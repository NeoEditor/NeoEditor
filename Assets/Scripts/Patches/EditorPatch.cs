using ADOFAI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace NeoEditor.Patches
{
	public static class EditorPatch
	{
		public static class ForceNeoEditor
		{
			// Not implemented in all methods. No duplicates.
			public static HashSet<string> notImplemented = new HashSet<string>();
			// Not implemented in current patching method. Key is field/method's name. Value means error(true) or warn(false).
			public static Dictionary<string, bool> notImplementedInCurrentMethod = new Dictionary<string, bool>();

			public static MethodInfo PatchMethod
			{
				get
				{
					return typeof(ForceNeoEditor).GetMethod("Transpiler", AccessTools.all);
				}
			}

			public static List<Type> GetTargetTypes()
			{
				List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
					.Where(asm => asm.FullName.Contains("Assembly-CSharp"))
					.SelectMany(asm => asm.GetTypes())
					.Where(t => t.IsClass && t.Namespace == "ADOFAI.LevelEditor.Controls")
					.ToList();
				types.Add(typeof(PropertiesPanel));

				return types;
			}

			public static List<MethodInfo> GetTargetMethods(Type type)
			{
				return type.GetDeclaredMethods();
			}

			public static void Patcher(Harmony harmony)
			{
				var types = GetTargetTypes();
				foreach (var type in types)
				{
					foreach (var method in GetTargetMethods(type))
					{
						try
						{
							//NeoLogger.Debug($"Patching {method.ReflectedType.Name}.{method.Name}.");
							notImplementedInCurrentMethod.Clear();
							harmony.Patch(method, transpiler: PatchMethod);
							if (notImplementedInCurrentMethod.Count > 0)
							{
								NeoLogger.Error($"Patching {method.ReflectedType.Name}.{method.Name}(). Has errors.");
								foreach (var kv in notImplementedInCurrentMethod)
								{
									if (kv.Value)
										NeoLogger.Error($"{kv.Key} is not implemented.");
									else
										NeoLogger.Warn($"{kv.Key} is not implemented.");
								}
								NeoLogger.LogEmpty(NeoLogger.LogLevel.Debug);
							}
						}
						catch (Exception e)
						{
							NeoLogger.Error($"Patching {method.ReflectedType.Name}.{method.Name} failed.");
							NeoLogger.Error(e);
						}
					}
				}

				NeoLogger.LogEmpty(NeoLogger.LogLevel.Debug);
				foreach (var s in notImplemented)
				{
					NeoLogger.Debug($"{s} is not implemented.");
				}
			}

			public static void Unpatcher(Harmony harmony)
			{
				var types = GetTargetTypes();
				foreach (var type in types)
				{
					foreach (var method in GetTargetMethods(type))
					{
						try
						{
							//NeoLogger.Debug($"Unpatching {method.ReflectedType.Name}.{method.Name}.");
							harmony.Unpatch(method, PatchMethod);
						}
						catch (Exception e)
						{
							NeoLogger.Error($"Unpatching {method.ReflectedType.Name}.{method.Name} failed.");
							NeoLogger.Error(e);
						}
					}
				}
			}

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];

					if (code.opcode == OpCodes.Call)
					{
						MethodInfo mi = code.operand as MethodInfo;
						if (mi?.ReturnType == typeof(scnEditor))
						{
							if (i + 4 < codes.Count && codes[i + 4].operand is ConstructorInfo)
							{
								ConstructorInfo ci = codes[i + 4].operand as ConstructorInfo;
								if (ci.DeclaringType == typeof(global::SaveStateScope))
								{
									continue;
								}
							}
							code.operand = typeof(NeoEditor).GetProperty("Instance").GetMethod;
							continue;
						}
						else if (mi?.ReturnType == typeof(RDConstants))
						{
							if (i + 1 < codes.Count && codes[i + 1].operand is FieldInfo)
							{
								FieldInfo fi = codes[i + 1].operand as FieldInfo;
								if (fi.Name.Contains("prefab_"))
								{
									FieldInfo prefab = typeof(NeoEditor).GetField(fi.Name, AccessTools.all);
									if (prefab == null)
									{
										//NeoLogger.Warn($"{fi.Name} is not implemented.");
										notImplementedInCurrentMethod.Add(fi.Name, false);
										notImplemented.Add(fi.Name);
									}
									else
									{
										code.operand = typeof(NeoEditor).GetProperty("Instance").GetMethod;
										codes[i + 1].operand = prefab;
									}
									continue;
								}
							}
						}
					}

					if (code.opcode == OpCodes.Ldsfld)
					{
						FieldInfo fi = code.operand as FieldInfo;
						if (fi?.FieldType == typeof(scnEditor))
						{
							code.opcode = OpCodes.Call;
							code.operand = typeof(NeoEditor).GetProperty("Instance").GetMethod;
							continue;
						}
					}

					if (code.operand is FieldInfo)
					{
						FieldInfo fieldInfo = code.operand as FieldInfo;
						if (fieldInfo.ReflectedType == typeof(scnEditor))
						{
							FieldInfo field = typeof(NeoEditor).GetField(fieldInfo.Name, AccessTools.all);
							if (field == null)
							{
								//NeoLogger.Error($"{fieldInfo.Name} is not implemented.");
								notImplementedInCurrentMethod.TryAdd(fieldInfo.Name, true);
								notImplemented.Add(fieldInfo.Name);
								//code.opcode = OpCodes.Nop;
							}
							else
								code.operand = field;
						}
					}
					else if (code.operand is MethodInfo)
					{
						MethodInfo methodInfo = code.operand as MethodInfo;
						if(methodInfo.ReflectedType == typeof(scnEditor))
						{
							MethodInfo method = typeof(NeoEditor).GetMethod(methodInfo.Name, 
								methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
							if (method == null)
							{
								//NeoLogger.Error($"{methodInfo.Name} is not implemented.");
								notImplementedInCurrentMethod.TryAdd(methodInfo.Name, true);
								notImplemented.Add(methodInfo.Name);
								//code.opcode = OpCodes.Nop;
							}
							else
								code.operand = method;
						}
					}
					//else if (code.operand is ConstructorInfo)
					//{
					//	ConstructorInfo constructorInfo = code.operand as ConstructorInfo;
					//	if (constructorInfo.DeclaringType == typeof(global::SaveStateScope))
					//	{
					//		ConstructorInfo constructor = typeof(SaveStateScope).GetConstructor(new Type[] { typeof(NeoEditor), typeof(bool), typeof(bool), typeof(bool) });
					//		code.operand = constructor;
					//	}
					//}
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PropertiesPanel), "Init")]
		public static class FloorNumProperty
		{
			public static void Prefix(LevelEventInfo levelEventInfo)
			{
				if (
					!GCS.settingsInfo.Values.Contains(levelEventInfo)
					&& !levelEventInfo.propertiesInfo.Keys.Contains("floor")
				)
				{
					Dictionary<string, object> dict = new Dictionary<string, object>
				{
					{ "name", "floor" },
					{ "type", "Int" },
					{ "default", 0 },
					{ "key", "editor.tileNumber" },
					{ "canBeDisabled", false }
				};
					ADOFAI.PropertyInfo propertyInfo = new ADOFAI.PropertyInfo(dict, levelEventInfo);
					levelEventInfo.propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>
					{
						{ "floor", propertyInfo }
					}.Concat(levelEventInfo.propertiesInfo).ToDictionary(k => k.Key, v => v.Value);
				}
			}
		}

		[HarmonyPatch(typeof(global::SaveStateScope), MethodType.Constructor, 
			new Type[] { typeof(scnEditor), typeof(bool), typeof(bool), typeof(bool) })]
		public static class SaveStateScopePatch
		{
			public static bool Prefix(bool clearRedo = false, bool dataHasChanged = true, bool skipSaving = false)
			{
				NeoEditor editor = NeoEditor.Instance;
				if (editor == null) return true;
				if (!skipSaving)
				{
					editor.SaveState(clearRedo, dataHasChanged);
				}
				editor.changingState++;
				return false;
			}
		}

		[HarmonyPatch(typeof(global::SaveStateScope), "Dispose")]
		public static class SaveStateDisposePatch
		{
			public static bool Prefix()
			{
				NeoEditor editor = NeoEditor.Instance;
				if (editor == null) return true;
				editor.changingState--;
				return false;
			}
		}
	}
}

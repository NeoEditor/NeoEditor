using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoEditor
{
	public class SaveStateScope : IDisposable
	{
		private NeoEditor editor;

		public SaveStateScope(NeoEditor editor, bool clearRedo = false, bool dataHasChanged = true, bool skipSaving = false)
		{
			this.editor = editor;
			if (!skipSaving)
			{
				//editor.SaveState(clearRedo, dataHasChanged);
			}

			//editor.changingState++;
		}

		public void Dispose()
		{
			//editor.changingState--;
		}
	}
}

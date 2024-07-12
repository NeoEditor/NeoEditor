using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoEditor
{
	public static class NeoUtils
	{
		public static void InsertionSort<T>(this List<T> a, Comparison<T> comparison)
		{
			T temp;
			for (int i = 1, j; i < a.Count; i++)
			{
				temp = a[i];
				for (j = i - 1; j >= 0; j--)
				{
					if (comparison.Invoke(a[j], temp) < 0) break;
					a[j + 1] = a[j];
				}
				a[j + 1] = temp;
			}
		}
	}
}

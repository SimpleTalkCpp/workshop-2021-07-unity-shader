using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class MyList<T>
{
	public int Count => m_count;
	public int Capacity => m_data.Length;
	public ref T this[int index] => ref m_data[index];

	public void Clear() {
		m_count = 0;
	}

	public ref T Push() {
		Resize(m_count + 1);
		return ref m_data[m_count - 1];
	}

	public void Resize(int n) {
		if (n < 0) n = 0;

		if (n >= m_data.Length) {
			Reserve(n);
		}

		m_count = n;
	}

	public void Reserve(int n) {
		n = NextPow2(n);
		var data = new T[n];

		for (int i = 0; i < m_count; i++)
			data[i] = m_data[i];

		m_data = data;
	}

	public void Copy(MyList<T> src) {
		Clear();
		AddRange(src);
	}

	public void Add(in T src) {
		Push() = src;
	}

	public void AddRange(MyList<T> src) {
		if (src == null) return;
		for (int i = 0; i < src.Count; i++) {
			Push() = src[i];
		}
	}

	public void RemoveRange(int index, int count) {
		int start = Mathf.Clamp(index,         0, m_count);
		int end   = Mathf.Clamp(index + count, 0, m_count);

		count = end - start;

		int newCount = m_count - count;
		if (newCount <= 0) {
			Clear();
			return;
		}

		int tailCount = m_count - end;
		if (tailCount == 0) {
			Resize(start);
			return;
		}

		var newData = new T[NextPow2(newCount)];
		for (int i = 0; i < start; i++) {
			newData[i] = m_data[i];
		}

		for (int i = 0; i < tailCount; i++) {
			newData[start + i] = m_data[end + i];
		}

		m_data  = newData;
		m_count = newCount;
	}

	static int NextPow2(int v) {
		if (v <= 0) return 0;
		v--;
		v|=v>>1;
		v|=v>>2;
		v|=v>>4;
		v|=v>>8;
		v|=v>>16;
		v++;
		return v;
	}

	public override string ToString() {
		var s = new StringBuilder();
		s.Append("[");
		for (int i = 0; i < m_count; i++) {
			if (i > 0) {
				s.Append(", ");
			}
			s.Append(m_data[i]);
		}
		s.Append("]");
		return s.ToString();
	}

	int m_count;
	T[] m_data = new T[0];
}

#if UNITY_EDITOR
public class MyList_UnitTest {
	public static bool Compare(MyList<int> list, int[] arr) {
		if (list.Count != arr.Length) return false;
		for (int i = 0; i < arr.Length; i++) {
			if (list[i] != arr[i]) return false;
		}
		return true;
	}

	public static void Check(MyList<int> list, int[] arr) {
		if (!Compare(list, arr)) {
			Debug.AssertFormat(false, $"{list}");
		}
	}

	[UnityEditor.MenuItem("MyUnitTest/MyList %t")]
	public static void UnitTest() {
		Debug.Log("MyUnitTest/MyList");

		var list = new MyList<int>();
		for (int i = 0; i < 10; i++) {
			list.Add(i);
		}

		Check(list, new int[]{0,1,2,3,4,5,6,7,8,9});

		list.RemoveRange(0,2);
		Check(list, new int[]{2,3,4,5,6,7,8,9});

		list.RemoveRange(1,3);
		Check(list, new int[]{2,6,7,8,9});

		list.RemoveRange(4,4);
		Check(list, new int[]{2,6,7,8});

		Debug.Log($"list = {list}");
	}
}
#endif


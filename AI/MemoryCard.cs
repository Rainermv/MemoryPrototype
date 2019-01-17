using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AIMemory{

	public class MemoryCard{
	
		public MemoryCard(int i, int v){
			Index = i;
			Value = v;
			Played = false;
		}
		
		public int Index;
		public int Value;
		public bool Played;
		
	}

}

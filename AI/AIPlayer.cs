using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AIMemory{

	[System.Serializable]
	public class AIPlayer{
	
		private List<MemoryCard> card_memory_list = new List<MemoryCard>();
		public List<MemoryCard> CardMemoryList{
					 	get {return card_memory_list;} 
					 	set {card_memory_list = value;}
					 }
					 
		public AIChromossome Chromossome;
			
		public int 	 MemMax	{
			get {return Chromossome.MemMax;}
			set {Chromossome.MemMax = value;}
		}
		public float MemChanceSelf 	{
			get {return Chromossome.MemChanceSelf;}
			set {Chromossome.MemChanceSelf = value;}
		}
		public float MemChanceOther {
			get {return Chromossome.MemChanceOther;}
			set {Chromossome.MemChanceOther = value;}
		}
		public float ForgetChance 	{
			get {return Chromossome.ForgetChance;}
			set {Chromossome.ForgetChance = value;}
		}
		
		public int Score {get; set;}
		public string Name {get; set;}
		
		private int victories;
		public int Victories {
			get {
				return victories;
			}
		}

		public int matches;

		public int Matches {
			get {
				return matches;
			}
		}

		public float ratio;

		public float Ratio {
			get {
				return ratio;
			}
		}

		public float ratio_score;

		public float RatioScore {
			get {
				return ratio_score;
			}
		}
		
		private float best_ratio;
		
		public AIPlayer (string name, bool rand){
			
			this.Name = name;
			
			Chromossome = new AIChromossome(rand);
		}
		
		public AIPlayer (string name, AIChromossome chromossome){
			
			this.Name = name;
			this.Chromossome = chromossome;
		}
		
		public void FillBasic(){
		
			MemMax = 10;
			MemChanceSelf = 0.5f;
			MemChanceOther = 0.2f;
			ForgetChance = 0.05f;	
			
			Name = "Vanilla";

		}
		
		public void FillHard(){
			
			MemMax = 15;
			MemChanceSelf = 0.8f;
			MemChanceOther = 0.5f;
			ForgetChance = 0.01f;	
			
			Name = "Hard";
			
		}
		
		public int FindMatch(MemoryCard card){
		
			if (CardMemoryList.Contains(card))
				return -1;
				
			for (int i = 0; i < CardMemoryList.Count; i++){
				
	
				// if the cards are different
				// AND they have the same value, return and forget
				if (  CardMemoryList[i].Value == card.Value){
					
					int match = CardMemoryList[i].Index;
										
					AIDebug.PlayLog(this.Name + " - Found from memory " + match+"("+ card.Value+")");
					
					Forget (card.Value);

					return match;
					
					
				}
				
			}
			
			return -1;
		
		}
		
		public int[] FindMatch(){
		
			//MemoryCard pair = new MemoryCard[2];
			
			for (int i = 0; i < CardMemoryList.Count; i++){
				
				for (int j = i; j < CardMemoryList.Count; j++){
					
					// if the cards are different
					// AND they have the same value, return and forget
					if (CardMemoryList[i] != CardMemoryList[j] &&
						CardMemoryList[i].Value == CardMemoryList[j].Value){
					
						int[] match = new int[2];
						
						match[0] = CardMemoryList[i].Index;
						match[1] = CardMemoryList[j].Index;
						
						AIDebug.PlayLog(this.Name + " - Found from memory (" + match[0]+","+match[1]);
				
						Forget (match[0],match[1]);
						
						
						return match;
					}
					
				}
				
			}
			
			return null;
			
		}
		
		public void Forget(int index){
		
			for (int i = 0; i < card_memory_list.Count; i++){
				if (CardMemoryList[i].Index == index){
					CardMemoryList.RemoveAt(i);
					AIDebug.PlayLog(this.Name + " - Forgot Card " + index);
					return;
				}
			}
		}
		
		public void Forget(int index1, int index2){
			Forget (index1);
			Forget (index2);
		}
		
		public void ForgetRandom(){
			
			for (int i = 0; i < card_memory_list.Count; i++){
				
				if (Random.Range (0f,1f) < ForgetChance){
					Forget(card_memory_list[i].Index);	
				}
				
			}
		}
		
		public void Memorize(MemoryCard card){
		
			// return if the memory is full
			if (card_memory_list.Count >= MemMax) return;
			if (CardMemoryList.Contains(card)) return;
			
			AIDebug.PlayLog(this.Name + " - Memorized Card " + card.Index);
			CardMemoryList.Add(card);
		}
		
		private void Memorize(MemoryCard c1, MemoryCard c2){
			Memorize(c1);
			Memorize(c2);
		}
		
		public void MemorizeSelf(MemoryCard c1, MemoryCard c2){
			
			if (Random.Range (0f,1f) < MemChanceSelf)
				Memorize(c1);
			if (Random.Range (0f,1f) < MemChanceSelf)
				Memorize(c2);
		}
		public void MemorizeOther(MemoryCard c1, MemoryCard c2){
			if (Random.Range (0f,1f) < MemChanceOther)
				Memorize(c1);
			if (Random.Range (0f,1f) < MemChanceOther)
				Memorize(c2);
		}
		
		public void DebugChromossome(){
			
			AIDebug.GenLog("+++++++++++++++++++");
			AIDebug.GenLog("Speciment Log: " + this.Name);
			AIDebug.GenLog ("Ratio: " + Ratio + " (" + RatioScore + ")");
			AIDebug.GenLog ("Best Ratio: " + best_ratio);
			AIDebug.GenLog("MM: " + MemMax);
			AIDebug.GenLog("MCS: " + MemChanceSelf);
			AIDebug.GenLog("MCO: " + MemChanceOther);
			
			AIDebug.GenLog("FC: " + ForgetChance);
			AIDebug.GenLog("+++++++++++++++++++");
			
		}
		
		public void DebugMemory(){
		
			if (AIDebug.enable_play_log == false) return;
			
			string log = Name + "'s memory: ";
			foreach (MemoryCard card in CardMemoryList){
			
				log += card.Index + "(" + card.Value + "), ";
			}
			
			AIDebug.PlayLog(log);
		
		
		}
		
		public void SignalMatch(bool victory){
			if (victory) victories++ ;
				matches++;
		}
		
		public void SignalTraining(){
		
			ratio = (float)Victories / (float)Matches;
			if (ratio > best_ratio) 
				best_ratio = ratio;
			ratio_score = Mathf.Abs(AIDarwin.GetInstance().TRatio - Ratio);
		}
		
	}
	
	


}

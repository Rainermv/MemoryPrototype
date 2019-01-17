using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AIMemory{

	
	public class AITrainingGame : MonoBehaviour{
	
		public List<AIPlayer> PlayerList {get; set;}
		
		public MemoryCard[] 	 GameCards    {get; set;}
		public AIPlayer 		 Player1	{get; set;}
		public AIPlayer 		 Player2	{get; set;}
		
		const int DECK_SIZE = 30;
		const int TRAINING_SESSION = 10;
		
		private AIPlayer CurrentPlayer;
		private AIPlayer OtherPlayer;
	//	private List<MemoryCard> AvailableCards = new List<MemoryCard>();
		private MemoryCard[] AvailableCards;
		int available_size = 0;
		
		private int game_count = 0;
		private float DebugTimer;
		private TimeValues turn_times = new TimeValues();
		
		private AIDarwin Darwin;
		
		void Awake(){
			Darwin = AIDarwin.GetInstance();
		}

		void Start(){
		
			PlayerList =  new List<AIPlayer>();
			Player1 = new AIPlayer("Naive",false);
			Player2 = GetPlayerFromGeneration();

			RestartGame();
		}
		
		void RestartGame(){

			turn_times.Clear();

			ResetTimer ();
			//AIDebug.Log("Setting up new Match");		
			GameCards = new MemoryCard[DECK_SIZE];
			
			InitDeck ();
			ShuffleValues ();
			
			Player1.Score = 0;
			Player2.Score = 0;
			
			int p = Random.Range (0,2);
			if (p == 0) {
				CurrentPlayer = Player1;
				OtherPlayer = Player2;
			}
			if (p == 1) {
				CurrentPlayer = Player2;
				OtherPlayer = Player1;
			}
		}
		
		AIPlayer GetPlayerFromGeneration(){
		
			AIPlayer P = Darwin.NextPlayer();
			//P.DebugChromossome();
			
			return P;
		}
		
		void ResetTimer(){
			DebugTimer = Time.timeSinceLevelLoad;
		}
		
		void PrintTime(string label){
			Debug.Log (label + " " + (Time.timeSinceLevelLoad - DebugTimer).ToString("0.0000"));
		}
		
		float GetTime(){
			Debug.Log (Time.timeSinceLevelLoad + " - " + DebugTimer + " = " + (Time.timeSinceLevelLoad - DebugTimer));
			//PrintTime ("foo");
			return Time.timeSinceLevelLoad - DebugTimer;
		}
		
		void InitDeck(){
			
			int v = 0;
			for (int i = 0; i < DECK_SIZE; i+= 2){
			
				GameCards[i] = new MemoryCard(i,v);
				GameCards[i+1] = new MemoryCard(i,v);

				v++;
			}
		
		}
		
		void ShuffleValues(){
		
			for (int i = 0; i < DECK_SIZE -1; i++){
			
				int rand_index = UnityEngine.Random.Range(i,DECK_SIZE);
				Swap (i,rand_index);
			}
		
		}
		
		void Swap(int i1, int i2){
		
			int temp_value = GameCards[i1].Value;
			GameCards[i1].Value = GameCards[i2].Value;
			GameCards[i2].Value = temp_value;
		
		}
		
		void AIDebugPrintDeck(bool reveal){
		
			AIDebug.MatchLog ("Deck State");
		
			int row = 0;
			string print = "";
			foreach (MemoryCard card in GameCards){
				
				if (reveal || card.Played == true)
					print += "[" + card.Value + "] ";
				else{
					print += "[X] ";
				}
				row++;
				
				if (row >= 6){
					AIDebug.MatchLog (print);
					print = "";
					row = 0;
				}
			
			}
			
			AIDebug.MatchLog ("========================");
		
		}
		
		void Update(){
			
			if (CurrentPlayer == null) 
				return;
			
			// the current player plays its turn until he misses the play
			bool player_scores = false;

			player_scores = Turn ();
			
			EndTurn(player_scores);
			
			//Debug.Log ("Turn Time: " + Time.deltaTime);
			turn_times.Add (Time.deltaTime);
		}
		
		// the current player plays a turn
		bool Turn(){
			AIDebug.PlayLog("------------------------------");
			AIDebug.PlayLog ( CurrentPlayer.Name + "'s Turn");
			
			float T2 = Time.realtimeSinceStartup;
			
			AIPick pick = PickMemory();
			
			AIDebug.PlayLog ("PICK MEMORY: " + (Time.realtimeSinceStartup - T2));
			
						
			if (pick == null){ 
				float T3 = Time.realtimeSinceStartup;
				pick = PickRandom();
				AIDebug.PlayLog ("PICK RAND: " + (Time.realtimeSinceStartup - T3));
			}
			
			float T4 = Time.realtimeSinceStartup;

			bool p = PlayCards(pick);
			
			AIDebug.PlayLog("PLAY CARDS: " + (Time.realtimeSinceStartup - T4));
			
			CurrentPlayer.DebugMemory();
			OtherPlayer.DebugMemory();
			
			//AvailableCards.Clear ();
			AvailableCards = null;
			
			return p;
		}
		
		// pick a random set of cards
		AIPick PickRandom(){
			
			if (AvailableCards == null)
				FindAvailableCards();
		
			AIDebug.PlayLog(CurrentPlayer.Name + " is picking random cards");
			
			AIPick pick = new AIPick();
			
			pick.Set (0,GetRandomAvailable());
			
			pick.Set (1,CurrentPlayer.FindMatch(GameCards[pick.Index(0)]));
			
			if (pick.Index(1) == -1) 
				pick.Set (1,GetRandomAvailable());
			
			return pick;
	
		}
		
		// get a random card index from the available list
		// remove the chosen card from the list
		int GetRandomAvailable(){
			
			//int available_index = Random.Range (0,AvailableCards.Count);
			int available_index = Random.Range (0,available_size);
			int card_index = AvailableCards[available_index].Index;
			
			AvailableCards[available_index] = AvailableCards[available_size-1];
			available_size -= 1;
			//AvailableCards.RemoveAt(available_index);
			
			return card_index;
			
		}
		
		AIPick PickMemory(){

			if (CurrentPlayer.CardMemoryList.Count < 2) 
				return null;
			
			int[] match = CurrentPlayer.FindMatch();
			
			if (match != null){
	
				OtherPlayer.Forget (match[0],match[1]);

				return new AIPick(match[0], match[1]);
			}
			
			return null;
		
		}
		
		// fill a list with all non-played cards
		void FindAvailableCards(){
		
			
			float T1 = Time.realtimeSinceStartup;
			
			/*AvailableCards.Clear();
			foreach (MemoryCard card in GameCards){
		
				if (!card.Played && !CurrentPlayer.CardMemoryList.Contains(card)){
					AvailableCards.Add (card);
				}
			}
			*/
			
			AvailableCards = new MemoryCard[GameCards.Length];
			
			available_size = 0;
			foreach (MemoryCard card in GameCards){
				
				if (!card.Played && !CurrentPlayer.CardMemoryList.Contains(card)){
					AvailableCards[available_size++] = card;
				}
			}
			
			AIDebug.PlayLog ("FIND AVAILABLE CARDS: " + (Time.realtimeSinceStartup - T1));
			

		}
		
		// play the picked cards
		bool PlayCards(AIPick pick){
		
			int ind1 = pick.Index (0);
			int ind2 = pick.Index (1);
		
			MemoryCard card1 = GameCards[ind1];
			MemoryCard card2 = GameCards[ind2];

			// if the values are equal, current player scores and plays again
			if (card1.Value == card2.Value){
			
				// both cards are set as "played"
				card1.Played = true;
				card2.Played = true;
				
				AIDebug.PlayLog ("SCORE (" + card1.Value + ") " + card1.Index + "-" + card2.Index);
				
				CurrentPlayer.Score += 2;
				
				return true;
			}
			
			AIDebug.PlayLog ("MISS  " + card1.Index + "(" + card1.Value + ") " + card2.Index + "(" + card2.Value + ") ");
			
			CurrentPlayer.MemorizeSelf(card1,card2);
			OtherPlayer.MemorizeOther(card1,card2);
			
			// the cards are not equal, player passes the turn
			return false;
		
		}
		
		void EndTurn(bool play_again){
		
			if (CurrentPlayer.Score > DECK_SIZE/2){
				EndGame();

				return;
			}
			
			if (play_again)
				return;
			
			if (CurrentPlayer == Player1){
				CurrentPlayer = Player2;
				OtherPlayer = Player1;
			}
			else{
				CurrentPlayer = Player1;
				OtherPlayer = Player2;
			}
		}
		
		// end the game and print the results
		void EndGame(){
			
			CurrentPlayer.SignalMatch(true);
			OtherPlayer.SignalMatch(false);
			
			AIDebug.MatchLog ("========================");
			AIDebug.MatchLog ("GAME " + (game_count+1) + " RESULTS");
			AIDebug.MatchLog (Player1.Name + ":: Score: " + Player1.Score + " Victories: " + Player1.Victories);
			AIDebug.MatchLog (Player2.Name + ":: Score: " + Player2.Score + " Victories: " + Player2.Victories);
			
			//PrintTime ("Elapsed match time:");
			AIDebug.MatchLog ("Average Turn Time: " + turn_times.Average);
			AIDebug.MatchLog ("Total Turn Time: " + turn_times.Sum);
			
			game_count++;
			
			CurrentPlayer.ForgetRandom();
			OtherPlayer.ForgetRandom();
			
			if (game_count >= TRAINING_SESSION){
			
				game_count = 0;
				
				EndTraining();
			
			}
			
			RestartGame();
		}
		
		void EndTraining(){
		
			AIDebug.GenLog
			("=====END OF TRAINING=====");
			
			CurrentPlayer.SignalTraining();
			OtherPlayer.SignalTraining();
			
			Player2.DebugChromossome();
			Player2 = GetPlayerFromGeneration();
	
		}
	
	}

}

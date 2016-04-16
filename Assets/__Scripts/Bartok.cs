using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//enum for phases of a turn
public enum TurnPhase{
	idle,
	pre,
	waiting,
	post,
	gameOver
}//end of TurnPhase

public class Bartok : MonoBehaviour {
	static public Bartok S;
	static public Player CURRENT_PLAYER;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCeter = Vector3.zero;
	public float handFanDegrees = 10f;
	public int numStartingCards = 7;
	public float drawTimeStagger = 0.1f;
	public bool ______________________;
	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;
	public List<Player> players;
	public CardBartok targetCard;

	public TurnPhase phase = TurnPhase.idle;
	public GameObject turnLight;

	public GameObject GTGameOver;
	public GameObject GTRoundResult;

	void Awake(){
		S = this;
		turnLight = GameObject.Find ("TurnLight");
		GTGameOver = GameObject.Find ("GTGameOver");
		GTRoundResult = GameObject.Find ("GTRoundResult");
		GTGameOver.SetActive (false);
		GTRoundResult.SetActive (false);
	}//end of Awake()

	void Start () {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);
		layout = GetComponent<BartokLayout> ();
		layout.ReadLayout (layoutXML.text);
		drawPile = UpgradeCardsList (deck.cards);
		LayoutGame ();
	}//end of Start()

	List<CardBartok> UpgradeCardsList(List<Card> lCD){
		List<CardBartok> lCB = new List<CardBartok>();
		foreach(Card tCD in lCD) lCB.Add(tCD as CardBartok);
		return lCB;
	}//end of UpgradeCardsList(List<Card> lCD)

	//udpate will be used to test adding cards to the players hand
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1))players [0].AddCard (Draw ());
		if (Input.GetKeyDown (KeyCode.Alpha2))players [1].AddCard (Draw ());
		if (Input.GetKeyDown (KeyCode.Alpha3))players [2].AddCard (Draw ());
		if (Input.GetKeyDown (KeyCode.Alpha4))players [3].AddCard (Draw ());
	}//end of Update()

	public void ArrangeDrawPile(){
		CardBartok tCB;

		for (int i = 0; i < drawPile.Count; i++) {
			tCB = drawPile [i];
			tCB.transform.parent = layoutAnchor;
			tCB.transform.localPosition = layout.drawPile.pos;
			tCB.faceUp = false;
			tCB.SetSortingLayerName (layout.drawPile.layerName);
			tCB.SetSortOrder (-i * 4);
			tCB.state = CBState.drawpile;
		}//end of for loop
	}//end of ArrangeDrawPile()

	void LayoutGame(){
		//if the anchor doesn't exist, then create it
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCeter;
		}//end of if

		//position the pile's cards
		ArrangeDrawPile();

		//set up the players
		Player p1;
		players = new List<Player> ();
		foreach (SlotDef tSD in layout.slotDefs) {
			p1 = new Player ();
			p1.handSlotDef = tSD;
			players.Add (p1);
			p1.playerNum = players.Count;
		}//end of foreach

		//1st player is human
		players[0].type = PlayerType.human;

		CardBartok tCB;

		//nested for loops to deal 7 cards to each player
		for (int i = 0; i < numStartingCards; i++) {
			for (int j = 0; j < 4; j++) {
				tCB = Draw ();
				tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
				players [(j + 1) % 4].AddCard (tCB);
			}//end of for j
		}//end of for i

		//after all the hand cards are drawn draw the target card
		Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
	}//end of LayoutGame()

	//flips up the first card
	public void DrawFirstTarget(){
		CardBartok tCb = MoveToTarget (Draw ());
		tCb.reportFinishTo = this.gameObject;
	}//end of DrawFirstTarget

	public void CBCallback(CardBartok cb){
		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.CBCallback()", cb.name);
		StartGame ();
	}//end of CBCallback

	public void StartGame(){
		PassTurn (1);
	}//end of StartGame()

	public void PassTurn(int num = -1){
		if (num == -1) {
			int ndx = players.IndexOf (CURRENT_PLAYER);
			num = (ndx + 1) % 4;
		}//end of if

		int lastPlayerNum = -1;

		if (CURRENT_PLAYER != null) {
			lastPlayerNum = CURRENT_PLAYER.playerNum;
			if (CheckGameOver ()) return;
		}
		CURRENT_PLAYER = players [num];
		phase = TurnPhase.pre;

		CURRENT_PLAYER.TakeTurn ();

		//this will move the turn light
		Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back*5;
		turnLight.transform.position = lPos;

		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
	}//end of PassTurn(int num = -1)

	public bool ValidPlay(CardBartok cb){
		if (cb.rank == targetCard.rank || cb.suit == targetCard.suit) return true;
		return false;
	}//end of ValidPlay

	//makes a new card the target
	public CardBartok MoveToTarget(CardBartok tCB){
		tCB.timeStart = 0;
		tCB.MoveTo (layout.discardPile.pos + Vector3.back);
		tCB.state = CBState.toTarget;
		tCB.faceUp = true;

		tCB.SetSortingLayerName ("10");
		tCB.eventualSortLayer = layout.target.layerName;

		if (targetCard != null) MoveToDiscard(targetCard);

		targetCard = tCB;

		return tCB;
	}//end of MoveToTarget(CardBartok tCB)

	public CardBartok Draw (){
		CardBartok cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}//end of Draw()


	public CardBartok MoveToDiscard(CardBartok tCB){
		tCB.state = CBState.discard;
		discardPile.Add (tCB);
		tCB.SetSortingLayerName (layout.discardPile.layerName);
		tCB.SetSortOrder (discardPile.Count * 4);
		tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

		return tCB;
	}//end of MoveToDiscard(CardBartok tCB)

	public void CardClicked(CardBartok tCB){
		//if it isn't the human's turn or if waiting for a card to move, do nothing
		if(CURRENT_PLAYER.type != PlayerType.human || phase == TurnPhase.waiting) return;

		//determine what was clicked to determine behavior
		switch(tCB.state){
		case CBState.drawpile:
			//draw the top card from the pile
			CardBartok cb = CURRENT_PLAYER.AddCard (Draw ());
			cb.callbackPlayer = CURRENT_PLAYER;
			Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CardClicked()","Draw",cb.name);
			phase = TurnPhase.waiting;
			break;
		case CBState.hand:
			if (ValidPlay (tCB)) {
				CURRENT_PLAYER.RemoveCard (tCB);
				MoveToTarget (tCB);
				tCB.callbackPlayer = CURRENT_PLAYER;
				Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.CardClicked()", "Play", tCB.name, targetCard.name + " is target");
				CURRENT_PLAYER.FanHand ();
			}//end of if
			break;
		}//end of switch
	}//end of CardClicked(CardBartok tCB)

	public bool CheckGameOver(){
		//do we need to reshuffle the discard pile into the draw pile?
		if (drawPile.Count == 0) {
			List<Card> cards = new List<Card> ();
			foreach (CardBartok cb in discardPile) cards.Add (cb);
			discardPile.Clear ();
			Deck.Shuffle (ref cards);
			drawPile = UpgradeCardsList (cards);
			ArrangeDrawPile ();
		}//end of if

		//has the current player won?
		if(CURRENT_PLAYER.hand.Count == 0){
			if (CURRENT_PLAYER.type == PlayerType.human) {
				GTGameOver.GetComponent<GUIText> ().text = "You Won!";
				GTRoundResult.GetComponent<GUIText> ().text = "";
			}//end of nested if
			else {
				GTGameOver.GetComponent<GUIText> ().text = "Game Over";
				GTRoundResult.GetComponent<GUIText> ().text = "Player " + CURRENT_PLAYER.playerNum + " won";
			}//end of else
			GTGameOver.SetActive (true);
			GTRoundResult.SetActive (true);
			phase = TurnPhase.gameOver;
			Invoke ("RestartGame", 3);
			return(true);
		}//end of if

		return false;
	}//end of CheckGameOver()

	public void RestartGame(){
		CURRENT_PLAYER = null;
		SceneManager.LoadScene ("__Bartok_Scene_0");
	}//end of RestartGame()
}//end of class
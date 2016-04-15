using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bartok : MonoBehaviour {
	static public Bartok S;

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

	void Awake(){
		S = this;
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
	}//end of DrawFirstTarget

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
}//end of class
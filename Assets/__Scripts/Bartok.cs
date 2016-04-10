using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bartok : MonoBehaviour {
	static public Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCeter = Vector3.zero;
	public bool ______________________;
	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;

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
	}//end of Start()

	List<CardBartok> UpgradeCardsList(List<Card> lCD){
		List<CardBartok> lCB = new List<CardBartok>();
		foreach(Card tCD in lCD) lCB.Add(tCD as CardBartok);
		return lCB;
	}//end of UpgradeCardsList(List<Card> lCD)

	void Update () {
	
	}//end of Update()
}//end of class
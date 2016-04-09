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

	void Awake(){
		S = this;
	}//end of Awake()

	void Start () {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);
	}//end of Start()

	void Update () {
	
	}//end of Update()
}//end of class
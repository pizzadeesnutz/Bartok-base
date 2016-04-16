using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerType{
	human,
	ai
}//end of PlayerType

[System.Serializable]
public class Player{
	public PlayerType type = PlayerType.ai;
	public int playerNum;

	public List<CardBartok> hand;

	public SlotDef handSlotDef;
	//add cards to hand
	public CardBartok AddCard(CardBartok eCB){
		if (hand == null) hand = new List<CardBartok> ();
		hand.Add (eCB);

		if(type == PlayerType.human){
			CardBartok[] cards = hand.ToArray ();
			cards = cards.OrderBy (cd => cd.rank).ToArray ();
			hand = new List<CardBartok> (cards);
		}

		eCB.SetSortingLayerName ("10");
		eCB.eventualSortLayer = handSlotDef.layerName;

		FanHand ();
		return(eCB);
	}//end of AddCard

	//remove a card from hand
	public CardBartok RemoveCard(CardBartok cb){
		hand.Remove (cb);
		return(cb);
	}//end of CardBartok

	public void FanHand(){
		float startRot = 0;
		startRot = handSlotDef.rot;
		if (hand.Count > 1) startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;

		Vector3 pos;
		float rot;
		Quaternion rotQ;

		for (int i = 0; i < hand.Count; i++) {
			rot = startRot - Bartok.S.handFanDegrees * i;
			rotQ = Quaternion.Euler (0, 0, rot);

			pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

			pos = rotQ * pos;

			pos += handSlotDef.pos;

			pos.z = -0.5f * i;

			if (Bartok.S.phase != TurnPhase.idle) hand [i].timeStart = 0;

			hand [i].MoveTo (pos, rotQ);
			hand [i].state = CBState.toHand;

//			hand [i].transform.localPosition = pos;
//			hand [i].transform.rotation = rotQ;
//			hand [i].state = CBState.hand;

			hand [i].faceUp = (type == PlayerType.human);

			//hand [i].SetSortOrder (i * 4);
			hand[i].eventualSortOrder = i * 4;
		}//end of for loop
	}//end of FanHand()

	public void TakeTurn(){
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.TakeTurn()");

		//the player can take his or her own action so end function
		if(type == PlayerType.human) return;
		Bartok.S.phase = TurnPhase.waiting;

		CardBartok cb;

		//find and store valid cards for AI
		List<CardBartok> validCards = new List<CardBartok>();
		foreach (CardBartok tCB in hand) {
			if (Bartok.S.ValidPlay (tCB)) validCards.Add (tCB);
		}//end of foreach

		//if no valid cards were found
		if (validCards.Count == 0) {
			cb = AddCard (Bartok.S.Draw ());
			cb.callbackPlayer = this;
			return;
		}//end of if

		//if one was found pick one at random
		cb = validCards[Random.Range(0,validCards.Count)];
		RemoveCard (cb);
		Bartok.S.MoveToTarget (cb);
		cb.callbackPlayer = this;
	}//end of TakeTurn()

	public void CBCallback(CardBartok tCB){
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
		Bartok.S.PassTurn ();
	}//end of CBCallback(CardBartok tCB)
}//end of class
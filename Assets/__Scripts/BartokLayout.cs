﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SlotDef{
	public float x;
	public float y;
	public bool faceUp = false;
	public string layerName = "Default";
	public int layerID = 0;
	public int id;
	public List<int> hiddenBy = new List<int>();
	public float rot;
	public string type = "slot";
	public Vector2 stagger;
	public int player;
	public Vector3 pos;
}//end of class SlotDef

public class BartokLayout : MonoBehaviour {
	public PT_XMLReader xmlr;
	public PT_XMLHashtable xml;
	public Vector2 multiplier;
	public List<SlotDef> slotDefs;
	public SlotDef drawPile;
	public SlotDef discardPile;
	public SlotDef target;

	public void ReadLayout(string xmlText){
		//bring in the XML and set a shortcut reference to it as xml
		xmlr = new PT_XMLReader ();
		xmlr.Parse (xmlText);
		xml = xmlr.xml ["xml"] [0];

		//set up multiplier
		multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
		multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

		SlotDef tSD;

		PT_XMLHashList slotsX = xml ["slot"];

		for (int i = 0; i < slotsX.Count; i++) {
			tSD = new SlotDef ();
			if (slotsX [i].HasAtt ("type")) tSD.type = slotsX [i].att ("type");
			else tSD.type = "slot";

			//parse various values into floats to assign them to the temporary slot definition
			tSD.x = float.Parse (slotsX [i].att ("x"));
			tSD.y = float.Parse (slotsX [i].att ("y"));
			tSD.pos = new Vector3 (tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

			//layer sorting
			//layers are assigned numerical values 1-10
			tSD.layerID = int.Parse(slotsX[i].att("layer"));
			tSD.layerName = tSD.layerID.ToString (); //converts layer number to a string

			switch (tSD.type) {
			case "slot":
				break;

			case "drawpile":
				tSD.stagger.x = float.Parse (slotsX [i].att ("xstagger"));
				drawPile = tSD;
				break;

			case "discardpile":
				discardPile = tSD;
				break;

			case "target":
				target = tSD;
				break;

			case "hand":
				tSD.player = int.Parse (slotsX [i].att ("player"));
				tSD.rot = float.Parse (slotsX [i].att ("rot"));
				slotDefs.Add (tSD);
				break;
			}//end of switch(tSD.type)
		}//end of for i
	}//end of ReadLayout(string xmlText)

	void Start () {
	
	}//end of Start()
	
	void Update () {
	
	}//end of Update()
}//end of class BartokLayout
using System;
using System.Collections.Generic;

partial class TelephoneList : ListElement {
	class TelephoneListItem : ListItem {
		public TelephoneListItem(string translationID, string room, string dialogueID) : base("", null) {
			text = Util.Tr(translationID);
			onClick = () => DialogueSystem.PrepareTelephoneCall(room, dialogueID);

			if (RoomManager.currentRoom != "RoomOffice")
				onClick += DialogueSystem.StartPreparedTelephoneCall;

			if (RoomManager.WasRoomVisited(room)) {
				visible = true;
			}
		}
	}

	public TelephoneList() {
		this.itemsCloseList = true;
		this.lines = new List<ListItem>(
			new ListItem[] {
				// Bank contacts
				new TelephoneListItem("Filo>2001", "RoomBank", "loanDialogue"),
				new TelephoneListItem("Filo>2002", "RoomBank", "stocksDialogue"),
				
				// Airport manager
				new TelephoneListItem("Filo>2003", "RoomManager", "managerDialogue"),
				
				// TODO: Implement the following phone contacts when their dialogues are ready:
				// new TelephoneListItem("Filo>2004", "RoomPlaneBroker", "planeBrokerDialogue"), // Plane broker
				// new TelephoneListItem("Filo>2005", "RoomMuseum", "museumDialogue"), // Museum
				// new TelephoneListItem("Filo>2006", "RoomNasaShop", "nasaShopDialogue"), // Nasa-Shop
				// new TelephoneListItem("Filo>2007", "RoomPersonnel", "seligDialogue"), // Personnel Office (Miss Selig)
				// new TelephoneListItem("Filo>2008", "RoomPersonnel", "hagedornDialogue"), // Personnel Office (Hagedorn)
				// new TelephoneListItem("Filo>2009", "RoomMarketing", "advertisingDialogue"), // Advertising agency
				// new TelephoneListItem("Filo>2010", "RoomWorkshop", "workshopDialogue"), // Workshop
				// new TelephoneListItem("Filo>2012", "RoomCompetitors", "competitorsDialogue"), // Competitors...
				// new TelephoneListItem("Filo>2013", "RoomBranches", "branchesDialogue"), // Branches...
			});
	}

}
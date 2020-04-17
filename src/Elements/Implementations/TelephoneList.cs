using System;
using System.Collections.Generic;

class TelephoneList : ListElement {
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
				new TelephoneListItem("Filo>2001", "RoomBank", "loanDialogue"),
				new TelephoneListItem("Filo>2002", "RoomBank", "stocksDialogue"),
				
				new TelephoneListItem("Filo>2003", "RoomManager", "managerDialogue"),
			});
	}

}
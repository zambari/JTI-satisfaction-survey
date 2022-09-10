using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using UnityEngine.UI;
using ToucanApp.States;

namespace ToucanApp.Data
{
    public class StateLinkContent : AbstractContent<StateLinkData>
    {
        public SubStateMachine linkedSubstate;
        private string stateId;

        public override void OnExportData ()
		{
			base.OnExportData ();

            var stateContent = linkedSubstate.GetComponent<StateContent>();

            if (stateContent != null)
                Data.stateID = stateContent.ID;
        }

        public override void OnContentChanged()
        {
            base.OnContentChanged();

            stateId = Data.stateID;
            if (!string.IsNullOrEmpty(stateId))
            {
                var stateContent = ContentHandler.GetContentWithID<StateContent>(stateId);

                if (stateContent != null)
                    linkedSubstate = stateContent.GetComponent<SubStateMachine>();
            }
        }

        public void EnterLinkedState()
		{
			if (linkedSubstate != null)
				linkedSubstate.EnterStateWithParents ();
		}
    }
}

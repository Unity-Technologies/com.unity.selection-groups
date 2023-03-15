using Unity.FilmInternalUtilities;

namespace Unity.SelectionGroups {

internal class SelectionGroupEnableEvent : AnalyticsEvent {

    internal SelectionGroupEnableEvent(int members, bool query) : base(new EventData { numMembers = members, isQuery = query }) { }

    private class EventData : AnalyticsEventData {
        public int numMembers;
        public bool isQuery;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "selectiongroups_sg_enable";
    internal override int    maxItems  => 1;

    
}

} //end namespace
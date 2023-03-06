using Unity.FilmInternalUtilities;

namespace Unity.SelectionGroups {

internal class SelectionGroupEnableEvent : AnalyticsEvent<SelectionGroupEnableEvent.EventData>
{

    internal struct EventData
    {
        public string converter;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "selectiongroups_sg_enable";
    internal override int    maxItems  => 10;

    
}

} //end namespace
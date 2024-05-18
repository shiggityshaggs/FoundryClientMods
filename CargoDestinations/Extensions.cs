namespace CargoDestinations
{
    static class DroneTransportGO_Extensions
    {
        internal static string StationName(this DroneTransportGO transport)
        {
            if (transport?.template == null) return string.Empty;
            StationType type = transport.template.droneTransport_isStartStation ? StationType.PickUp : StationType.DropOff;
            return Helpers.GetStationName(transport.relatedEntityId, type);
        }
    }
}

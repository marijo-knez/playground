using System;
using Logistics.Domain.Base;

namespace Logistics.Domain.Import.ShipmentRoute;

public class TransportStatusChangedDomainEvent: IDomainEvent
{
    public int TransportId { get; private set; }
    public int TransportStatusId {get; private set;}

    public TransportStatusChangedDomainEvent(int transportId, int transportStatusId) {
        TransportId = transportId;
        TransportStatusId = transportStatusId;
    }
}

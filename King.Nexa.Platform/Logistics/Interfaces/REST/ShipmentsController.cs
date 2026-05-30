using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Logistics.Interfaces.REST.Resources;
using King.Nexa.Platform.Logistics.Interfaces.REST.Transform;
using King.Nexa.Platform.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace King.Nexa.Platform.Logistics.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
public class ShipmentsController(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllShipments(CancellationToken cancellationToken)
    {
        var shipments = await shipmentRepository.ListAsync(cancellationToken);
        return Ok(shipments.Select(ShipmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShipmentById(int id, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FindByIdAsync(id, cancellationToken);
        return shipment is null ? NotFound() : Ok(ShipmentResourceFromEntityAssembler.ToResourceFromEntity(shipment));
    }

    [HttpPost]
    public async Task<IActionResult> ScheduleShipment(ScheduleShipmentResource resource, CancellationToken cancellationToken)
    {
        var command = ScheduleShipmentCommandFromResourceAssembler.ToCommandFromResource(resource);
        var shipment = new Shipment(command);
        await shipmentRepository.AddAsync(shipment, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return CreatedAtAction(nameof(GetShipmentById), new { id = shipment.Id }, ShipmentResourceFromEntityAssembler.ToResourceFromEntity(shipment));
    }

    [HttpPost("{id:int}/delivered")]
    public async Task<IActionResult> MarkShipmentDelivered(int id, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FindByIdAsync(id, cancellationToken);
        if (shipment is null) return NotFound();
        shipment.MarkDelivered();
        shipmentRepository.Update(shipment);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Ok(ShipmentResourceFromEntityAssembler.ToResourceFromEntity(shipment));
    }
}

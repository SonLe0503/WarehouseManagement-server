using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutboundRequestController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;
    

    public OutboundRequestController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OutboundRequestDTO>>> GetOutboundRequests()
        {
            var requests = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .ToListAsync();

            var requestDtos = _mapper.Map<List<OutboundRequestDTO>>(requests);
            return Ok(requestDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OutboundRequestDTO>> GetOutboundRequestDetails(int id)
        {
            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var requestDto = _mapper.Map<OutboundRequestDTO>(request);
            return Ok(requestDto);
        }
    }

}


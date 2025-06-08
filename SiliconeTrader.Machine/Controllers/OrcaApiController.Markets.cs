using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Added for async
using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.Core;
using SiliconeTrader.Core.Models; // For MarketPairsRequest, MarketPairsResponse etc.
using SiliconeTrader.Machine.Client.Models; // For MarketSignalsResponse

namespace SiliconeTrader.Machine.Controllers
{
    public partial class OrcaApiController
    {
        [HttpPost("market-pairs")]
        [ProducesResponseType(200, Type = typeof(MarketPairsResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MarketPairsResponse>> MarketPairs([FromBody] MarketPairsRequest request)
        {
            if (request?.SignalsFilter == null)
            {
                return BadRequest("Request or SignalsFilter cannot be null.");
            }

            var response = await _marketDataService.GetMarketPairsAsync(request);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpGet("market-signals")]
        [ProducesResponseType(200, Type = typeof(MarketSignalsResponse))]
        public MarketSignalsResponse MarketSignals()
        {
            // Use injected _signalsService
            return new MarketSignalsResponse
            {
                Signals = _signalsService.GetSignalNames()
            };
        }
    }
}
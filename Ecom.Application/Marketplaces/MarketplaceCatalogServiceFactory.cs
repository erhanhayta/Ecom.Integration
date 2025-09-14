using System;
using System.Collections.Generic;
using Ecom.Domain.Marketplaces;

namespace Ecom.Application.Marketplaces
{
    public sealed class MarketplaceCatalogServiceFactory : IMarketplaceCatalogServiceFactory
    {
        private readonly IEnumerable<IMarketplaceCatalogService> _services;
        public MarketplaceCatalogServiceFactory(IEnumerable<IMarketplaceCatalogService> services) => _services = services;

        public IMarketplaceCatalogService Get(Firm firm) =>
            _services.FirstOrDefault(s => s.Firm == firm)
                ?? throw new InvalidOperationException($"No catalog service registered for {firm}");
    }
}

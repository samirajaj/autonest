import { useDeferredValue, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { RotateCcw, Search, SlidersHorizontal } from "lucide-react";
import { api } from "../../shared/api/client";
import type { Car, Paged } from "../../shared/types";
import { CarCard } from "../../shared/components/CarCard";
import { useAuth } from "../../app/AuthContext";

export function CarsPage() {
  const [filters, setFilters] = useState({
    search: "",
    type: "",
    fuelType: "",
    gearType: "",
    isForSale: "",
  });
  const deferredSearch = useDeferredValue(filters.search.trim());

  const params = new URLSearchParams(
    Object.entries({ ...filters, search: deferredSearch }).filter(([, v]) => v),
  );
  const hasFilters = Object.values(filters).some(Boolean);

  const { data, isLoading, error } = useQuery({
    queryKey: ["cars", params.toString()],
    queryFn: () => api<Paged<Car>>(`/cars?${params}`),
  });

  const { session } = useAuth();
  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">CURATED MARKETPLACE</p>
          <h1>Cars worth your attention.</h1>
          <p className="muted">Search verified listings for rent or sale.</p>
        </div>
        <span className="muted">{data?.totalCount ?? 0} vehicles</span>
      </div>
      <form className="card filters" role="search" onSubmit={(e) => e.preventDefault()}>
        <div className="filter-heading">
          <SlidersHorizontal size={18} />
          <span>Refine results</span>
        </div>
        <label className="filter-control filter-search">
          <span>Search</span>
          <span className="search-field">
            <Search />
            <input
              type="search"
              placeholder="Make or model"
              value={filters.search}
              maxLength={100}
              autoComplete="off"
              onChange={(e) => setFilters({ ...filters, search: e.target.value })}
            />
          </span>
        </label>
        <label className="filter-control">
          <span>Body</span>
          <select value={filters.type} onChange={(e) => setFilters({ ...filters, type: e.target.value })}>
            <option value="">All bodies</option>
            {["Sedan", "SUV", "Truck", "Van", "Coupe", "Convertible", "Hatchback"].map((x) => (
              <option key={x}>{x}</option>
            ))}
          </select>
        </label>
        <label className="filter-control">
          <span>Fuel</span>
          <select value={filters.fuelType} onChange={(e) => setFilters({ ...filters, fuelType: e.target.value })}>
            <option value="">All fuels</option>
            {["Gasoline", "Diesel", "Electric", "Hybrid", "PlugInHybrid"].map((x) => (
              <option key={x}>{x}</option>
            ))}
          </select>
        </label>
        <label className="filter-control">
          <span>Gearbox</span>
          <select value={filters.gearType} onChange={(e) => setFilters({ ...filters, gearType: e.target.value })}>
            <option value="">All gearboxes</option>
            {["Automatic", "Manual", "CVT", "SemiAutomatic"].map((x) => (
              <option key={x}>{x}</option>
            ))}
          </select>
        </label>
        <label className="filter-control">
          <span>Listing</span>
          <select value={filters.isForSale} onChange={(e) => setFilters({ ...filters, isForSale: e.target.value })}>
            <option value="">Rent or buy</option>
            <option value="true">For sale</option>
            <option value="false">For rent</option>
          </select>
        </label>
        <button
          className="filter-reset"
          type="button"
          disabled={!hasFilters}
          onClick={() => setFilters({ search: "", type: "", fuelType: "", gearType: "", isForSale: "" })}
        >
          <RotateCcw size={16} /> Clear
        </button>
      </form>
      {isLoading ? (
        <div className="empty">Loading the latest vehicles…</div>
      ) : error ? (
        <div className="error">{error.message}</div>
      ) : data?.items.length ? (
        <div className="grid">
          {data.items.map((car) => (
            <CarCard
              key={car.id}
              car={car}
              onFavorite={session?.role === "Customer" ? () => {} : undefined}
            />
          ))}
        </div>
      ) : (
        <div className="empty">No vehicles match those filters.</div>
      )}
    </div>
  );
}

import { RotateCcw, Search, SlidersHorizontal } from "lucide-react";

export interface Filters {
  search: string;
  type: string;
  fuelType: string;
  gearType: string;
  isForSale: string;
}

interface FilterFormProps {
  filters: Filters;
  onChange: (filters: Filters) => void;
  onReset: () => void;
  hasFilters: boolean;
  showHeading?: boolean;
}

export function FilterForm({
  filters,
  onChange,
  onReset,
  hasFilters,
  showHeading = true,
}: FilterFormProps) {
  return (
    <>
      {showHeading && (
        <div className="filter-heading">
          <SlidersHorizontal size={18} />
          <span>Refine results</span>
        </div>
      )}
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
            onChange={(e) => onChange({ ...filters, search: e.target.value })}
          />
        </span>
      </label>
      <label className="filter-control">
        <span>Body</span>
        <select value={filters.type} onChange={(e) => onChange({ ...filters, type: e.target.value })}>
          <option value="">All bodies</option>
          {["Sedan", "SUV", "Truck", "Van", "Coupe", "Convertible", "Hatchback"].map((x) => (
            <option key={x}>{x}</option>
          ))}
        </select>
      </label>
      <label className="filter-control">
        <span>Fuel</span>
        <select value={filters.fuelType} onChange={(e) => onChange({ ...filters, fuelType: e.target.value })}>
          <option value="">All fuels</option>
          {["Gasoline", "Diesel", "Electric", "Hybrid", "PlugInHybrid"].map((x) => (
            <option key={x}>{x}</option>
          ))}
        </select>
      </label>
      <label className="filter-control">
        <span>Gearbox</span>
        <select value={filters.gearType} onChange={(e) => onChange({ ...filters, gearType: e.target.value })}>
          <option value="">All gearboxes</option>
          {["Automatic", "Manual", "CVT", "SemiAutomatic"].map((x) => (
            <option key={x}>{x}</option>
          ))}
        </select>
      </label>
      <label className="filter-control">
        <span>Listing</span>
        <select value={filters.isForSale} onChange={(e) => onChange({ ...filters, isForSale: e.target.value })}>
          <option value="">Rent or buy</option>
          <option value="true">For sale</option>
          <option value="false">For rent</option>
        </select>
      </label>
      <button
        className="filter-reset"
        type="button"
        disabled={!hasFilters}
        onClick={onReset}
      >
        <RotateCcw size={16} /> Clear
      </button>
    </>
  );
}

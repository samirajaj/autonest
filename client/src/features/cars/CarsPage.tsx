import { useDeferredValue, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { SlidersHorizontal } from "lucide-react";
import { api } from "../../shared/api/client";
import type { Car, Paged } from "../../shared/types";
import { CarCard } from "../../shared/components/CarCard";
import { Modal } from "../../shared/components/Modal";
import { FilterForm, type Filters } from "../../shared/components/FilterForm";
import { useAuth } from "../../app/AuthContext";

const EMPTY_FILTERS: Filters = { search: "", type: "", fuelType: "", gearType: "", isForSale: "" };

export function CarsPage() {
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS);
  const [filterOpen, setFilterOpen] = useState(false);
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
      <button
        className="button secondary filter-mobile-toggle"
        type="button"
        onClick={() => setFilterOpen(true)}
      >
        <SlidersHorizontal size={16} /> Filters
        {hasFilters && <span className="filter-badge" />}
      </button>
      <form className="card filters filters-desktop" role="search" onSubmit={(e) => e.preventDefault()}>
        <FilterForm
          filters={filters}
          onChange={setFilters}
          onReset={() => setFilters(EMPTY_FILTERS)}
          hasFilters={hasFilters}
        />
      </form>
      {filterOpen && (
        <Modal title="Filters" onClose={() => setFilterOpen(false)}>
          <form className="form-panel form-grid" onSubmit={(e) => e.preventDefault()}>
            <FilterForm
              filters={filters}
              onChange={setFilters}
              onReset={() => setFilters(EMPTY_FILTERS)}
              hasFilters={hasFilters}
              showHeading={false}
            />
          </form>
        </Modal>
      )}
      {isLoading ? (
        <div className="empty">Loading the latest vehicles…</div>
      ) : error ? (
        <div className="empty">
          <p>Failed to load vehicles.</p>
          <p className="muted">Check your connection and try again.</p>
        </div>
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

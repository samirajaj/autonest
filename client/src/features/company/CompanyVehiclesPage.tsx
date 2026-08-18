import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { Plus, RotateCcw, Trash2 } from "lucide-react";
import { api, imageUrl } from "../../shared/api/client";
import type { Car, Paged } from "../../shared/types";
import { Modal } from "../../shared/components/Modal";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

export function CompanyVehiclesPage() {
  const [deleted, setDeleted] = useState(false);
  const [editing, setEditing] = useState<Car | null | undefined>();
  const actionRequest = useAsyncAction();

  const { data, refetch } = useQuery({
    queryKey: ["company-cars", deleted],
    queryFn: () => api<Paged<Car>>(`/company/cars?deleted=${deleted}`),
  });

  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const body = new FormData(e.currentTarget);
    const succeeded = await actionRequest.run("save-vehicle", async () => {
      const images = body
        .getAll("images")
        .filter((entry): entry is File => entry instanceof File && entry.size > 0);
      if (images.length > 5) throw new Error("Upload no more than 5 images.");
      if (images.some((image) => image.size > 5 * 1024 * 1024))
        throw new Error("Each image must be 5 MB or smaller.");
      if (images.some((image) => !["image/jpeg", "image/png", "image/webp"].includes(image.type)))
        throw new Error("Images must be JPEG, PNG, or WebP files.");

      await api(editing ? `/company/cars/${editing.id}` : "/company/cars", {
        method: editing ? "PUT" : "POST",
        body,
      });
    });
    if (succeeded) {
      setEditing(undefined);
      await refetch();
    }
  }

  async function action(car: Car, restore = false) {
    await actionRequest.run(`${restore ? "restore" : "delete"}-${car.id}`, async () => {
      await api(`/company/cars/${car.id}${restore ? "/restore" : ""}`, {
        method: restore ? "PUT" : "DELETE",
      });
      await refetch();
    });
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">FLEET MANAGEMENT</p>
          <h1>Your vehicles.</h1>
        </div>
        <div className="toolbar">
          <div className="tabs">
            <button
              className={!deleted ? "active" : ""}
              onClick={() => setDeleted(false)}
            >
              Active
            </button>
            <button
              className={deleted ? "active" : ""}
              onClick={() => setDeleted(true)}
            >
              Trash
            </button>
          </div>
          <button
            className="button"
            type="button"
            onClick={() => {
              actionRequest.clearError();
              setEditing(null);
            }}
          >
            <Plus /> Add vehicle
          </button>
        </div>
      </div>
      {actionRequest.error && <p className="error">{actionRequest.error}</p>}
      {data?.items.length ? (
        <div className="management-grid">
          {data.items.map((car) => (
            <article className="card management-car" key={car.id}>
              <img src={imageUrl(car.imageUrl)} alt="" />
              <div>
                <p className="eyebrow">
                  {car.isForSale ? "FOR SALE" : "FOR RENT"}
                </p>
                <h3>
                  {car.make} {car.model}
                </h3>
                <p className="muted">
                  {car.year} · {car.mileage.toLocaleString()} km · $
                  {car.price.toLocaleString()}
                </p>
              </div>
              <div className="toolbar">
                {deleted ? (
                  <ActionButton
                    className="button secondary"
                    pending={actionRequest.isPending(`restore-${car.id}`)}
                    pendingLabel="Restoring…"
                    onClick={() => action(car, true)}
                  >
                    <RotateCcw /> Restore
                  </ActionButton>
                ) : (
                  <>
                    <button
                      className="button secondary"
                      onClick={() => {
                        actionRequest.clearError();
                        setEditing(car);
                      }}
                    >
                      Edit
                    </button>
                    <ActionButton
                      className="icon-button danger"
                      pending={actionRequest.isPending(`delete-${car.id}`)}
                      pendingLabel=""
                      aria-label={`Delete ${car.make} ${car.model}`}
                      onClick={() => action(car)}
                    >
                      <Trash2 />
                    </ActionButton>
                  </>
                )}
              </div>
            </article>
          ))}
        </div>
      ) : (
        <div className="empty">
          {deleted
            ? "No deleted vehicles."
            : "Add your first vehicle to begin."}
        </div>
      )}
      {editing !== undefined && (
        <Modal
          title={editing ? "Edit vehicle" : "Add vehicle"}
          onClose={() => !actionRequest.isPending() && setEditing(undefined)}
        >
          <form className="form-panel form-grid" onSubmit={save}>
            {actionRequest.error && <p className="error field full">{actionRequest.error}</p>}
            <div className="field">
              <label>Make</label>
              <input name="make" defaultValue={editing?.make} minLength={2} maxLength={100} required />
            </div>
            <div className="field">
              <label>Model</label>
              <input name="model" defaultValue={editing?.model} minLength={1} maxLength={100} required />
            </div>
            <div className="field">
              <label>Year</label>
              <input
                name="year"
                type="number"
                defaultValue={editing?.year ?? new Date().getFullYear()}
                min="1900"
                max="2100"
                required
              />
            </div>
            <div className="field">
              <label>Body type</label>
              <select name="type" defaultValue={editing?.type}>
                {[
                  "Sedan",
                  "SUV",
                  "Truck",
                  "Van",
                  "Coupe",
                  "Convertible",
                  "Hatchback",
                ].map((x) => (
                  <option key={x}>{x}</option>
                ))}
              </select>
            </div>
            <div className="field">
              <label>Gearbox</label>
              <select name="gearType" defaultValue={editing?.gearType}>
                {["Automatic", "Manual", "CVT", "SemiAutomatic"].map((x) => (
                  <option key={x}>{x}</option>
                ))}
              </select>
            </div>
            <div className="field">
              <label>Fuel</label>
              <select name="fuelType" defaultValue={editing?.fuelType}>
                {[
                  "Gasoline",
                  "Diesel",
                  "Electric",
                  "Hybrid",
                  "PlugInHybrid",
                ].map((x) => (
                  <option key={x}>{x}</option>
                ))}
              </select>
            </div>
            <div className="field">
              <label>Seats</label>
              <input
                name="seatsCount"
                type="number"
                defaultValue={editing?.seatsCount ?? 5}
                min="1"
                max="100"
                required
              />
            </div>
            <div className="field">
              <label>Mileage</label>
              <input
                name="mileage"
                type="number"
                defaultValue={editing?.mileage ?? 0}
                min="0"
                max="10000000"
                required
              />
            </div>
            <div className="field">
              <label>Price</label>
              <input
                name="price"
                type="number"
                step="0.01"
                defaultValue={editing?.price ?? 0}
                min="0"
                max="1000000000"
                required
              />
            </div>
            <div className="field">
              <label>Listing</label>
              <select
                name="isForSale"
                defaultValue={String(editing?.isForSale ?? false)}
              >
                <option value="false">For rent</option>
                <option value="true">For sale</option>
              </select>
            </div>
            <input type="hidden" name="isAvailable" value="true" />
            <div className="field full">
              <label>Vehicle images</label>
              <input
                name="images"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                multiple
              />
              <small>Up to 5 JPEG, PNG, or WebP images, maximum 5 MB each.</small>
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={actionRequest.isPending("save-vehicle")}
              pendingLabel="Saving vehicle…"
            >
              Save vehicle
            </ActionButton>
          </form>
        </Modal>
      )}
    </div>
  );
}

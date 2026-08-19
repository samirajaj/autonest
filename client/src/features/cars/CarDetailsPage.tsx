import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Calendar,
  Check,
  ChevronLeft,
  Fuel,
  Gauge,
  Settings2,
  Users,
} from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { api, imageUrl } from "../../shared/api/client";
import type { Car } from "../../shared/types";
import { useAuth } from "../../app/AuthContext";
import { Modal } from "../../shared/components/Modal";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

export function CarDetailsPage() {
  const { id } = useParams();
  const { session } = useAuth();
  const [book, setBook] = useState(false);
  const [message, setMessage] = useState("");
  const [requestType, setRequestType] = useState<"Rent" | "Sale">("Rent");
  const action = useAsyncAction();

  const { data: car, isLoading } = useQuery({
    queryKey: ["car", id],
    queryFn: () => api<Car>(`/cars/${id}`),
  });

  async function request(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.currentTarget));
    setMessage("");

    const succeeded = await action.run("request", async () => {
      if (
        f.type === "Rent" &&
        (!f.startDate || !f.endDate || String(f.endDate) <= String(f.startDate))
      ) {
        throw new Error("Rental end date must be after the start date.");
      }

      await api("/requests", {
        method: "POST",
        body: JSON.stringify({
          carId: Number(id),
          type: f.type,
          startDate: f.startDate || null,
          endDate: f.endDate || null,
        }),
      });
    });

    if (succeeded) {
      setMessage("Your request was sent to the company.");
      setBook(false);
    }
  }

  if (isLoading || !car)
    return <div className="page empty">Loading vehicle…</div>;

  const images = car.imageUrls?.length ? car.imageUrls : [car.imageUrl];
  return (
    <div className="page details">
      <Link to="/cars" className="back">
        <ChevronLeft /> Back to marketplace
      </Link>
      <div className="details-grid">
        <div className="gallery">
          <img src={imageUrl(images[0])} alt={`${car.make} ${car.model}`} />
        </div>
        <aside className="card details-panel">
          <p className="eyebrow">{car.company}</p>
          <h1>
            {car.make} {car.model}
          </h1>
          <p className="price large">${car.price.toLocaleString()}</p>
          <div className="spec-grid">
            <span>
              <Calendar />
              {car.year}
            </span>
            <span>
              <Gauge />
              {car.mileage.toLocaleString()} km
            </span>
            <span>
              <Fuel />
              {car.fuelType}
            </span>
            <span>
              <Settings2 />
              {car.gearType}
            </span>
            <span>
              <Users />
              {car.seatsCount} seats
            </span>
            <span>
              <Check />
              {car.isForSale ? "For sale" : "For rent"}
            </span>
          </div>
          {message && <p className="success">{message}</p>}
          {session?.role === "Customer" ? (
            <button
              className="button"
              type="button"
              onClick={() => {
                action.clearError();
                setRequestType(car.isForSale ? "Sale" : "Rent");
                setBook(true);
              }}
            >
              Request this vehicle
            </button>
          ) : !session ? (
            <Link className="button" to="/auth">
              Sign in to request
            </Link>
          ) : null}
        </aside>
      </div>
      {book && (
        <Modal
          title="Request vehicle"
          onClose={() => !action.isPending() && setBook(false)}
        >
          <form className="form-panel form-grid" onSubmit={request}>
            {action.error && <p className="error field full">{action.error}</p>}
            <div className="field full">
              <label>Request type</label>
              <select
                name="type"
                value={requestType}
                disabled={action.isPending()}
                onChange={(e) => setRequestType(e.target.value as "Rent" | "Sale")}
                required
              >
                <option>Rent</option>
                <option>Sale</option>
              </select>
            </div>
            <div className="field">
              <label>Rental start</label>
              <input
                name="startDate"
                type="date"
                min={new Date().toISOString().slice(0, 10)}
                required={requestType === "Rent"}
                disabled={requestType !== "Rent" || action.isPending()}
              />
            </div>
            <div className="field">
              <label>Rental end</label>
              <input
                name="endDate"
                type="date"
                min={new Date().toISOString().slice(0, 10)}
                required={requestType === "Rent"}
                disabled={requestType !== "Rent" || action.isPending()}
              />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("request")}
            >
              Send request
            </ActionButton>
          </form>
        </Modal>
      )}
    </div>
  );
}

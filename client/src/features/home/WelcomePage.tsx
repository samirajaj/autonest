import { ArrowRight, CheckCircle2, ShieldCheck, Sparkles } from "lucide-react";
import { Link } from "react-router-dom";

export function WelcomePage() {
  return (
    <>
      <section className="hero">
        <div className="hero-image" />
        <div className="hero-shade" />
        <div className="hero-content">
          <p className="eyebrow">THE SMARTER WAY TO MOVE</p>
          <h1>
            Find the car that
            <br />
            <em>fits your story.</em>
          </h1>
          <p>
            Explore trusted vehicles from verified companies. Rent for the
            journey or buy for what comes next.
          </p>
          <div className="toolbar">
            <Link className="button" to="/cars">
              Explore vehicles <ArrowRight size={18} />
            </Link>
            <Link className="button secondary" to="/auth">
              Create account
            </Link>
          </div>
          <div className="hero-trust">
            <span>
              <CheckCircle2 /> Verified companies
            </span>
            <span>
              <ShieldCheck /> Secure requests
            </span>
            <span>
              <Sparkles /> Clear pricing
            </span>
          </div>
        </div>
      </section>
      <section className="page intro">
        <div>
          <p className="eyebrow">BUILT AROUND THE DRIVER</p>
          <h2>Less noise. Better decisions.</h2>
        </div>
        <p className="muted">
          AutoNest brings discovery, requests, favorites and transaction history
          into one considered experience—so every choice feels informed.
        </p>
      </section>
    </>
  );
}

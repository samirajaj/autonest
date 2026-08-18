import { Link } from "react-router-dom";

export function PrivacyPage() {
  return (
    <article className="page prose">
      <p className="eyebrow">PRIVACY</p>
      <h1>Your information, handled clearly.</h1>
      <p>
        AutoNest uses account information to provide authentication, vehicle
        requests, favorites, profile management, and transaction history.
        Contact information is used for required account and request
        notifications.
      </p>
      <p>
        Access is limited by account role. You may update your profile or
        request account deletion from your profile, subject to
        transaction-history requirements.
      </p>
    </article>
  );
}

export function NotFoundPage() {
  return (
    <div className="page not-found">
      <p className="eyebrow">404</p>
      <h1>That road ends here.</h1>
      <p className="muted">The page you requested does not exist.</p>
      <Link className="button" to="/">
        Return home
      </Link>
    </div>
  );
}

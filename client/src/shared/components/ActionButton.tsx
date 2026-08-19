import { LoaderCircle } from "lucide-react";
import type { ButtonHTMLAttributes, ReactNode } from "react";

type ActionButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  pending?: boolean;
  children: ReactNode;
};

export function ActionButton({
  pending = false,
  disabled,
  children,
  type = "button",
  ...props
}: ActionButtonProps) {
  return (
    <button
      {...props}
      type={type}
      disabled={disabled || pending}
      aria-busy={pending}
    >
      {pending ? <LoaderCircle className="button-spinner" size={17} /> : children}
    </button>
  );
}

const registry = new Map();

export function register(id, dotNetRef, element) {
  function handler(event) {
    if (element && !element.contains(event.target)) {
      dotNetRef.invokeMethodAsync("OnOutsideClick");
    }
  }
  document.addEventListener("mousedown", handler);
  registry.set(id, handler);
}

export function unregister(id) {
  const handler = registry.get(id);
  if (handler) {
    document.removeEventListener("mousedown", handler);
    registry.delete(id);
  }
}

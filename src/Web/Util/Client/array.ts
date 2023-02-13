export { }
declare global {
  interface Array<T> {
    toSorted(lambda: (item: T) => any): Array<T>
  }
}

Array.prototype.toSorted = function (lambda) {
  return this.slice(0).sort((a, b) =>
    lambda(a) > lambda(b) ? 1 : lambda(a) < lambda(b) ? -1 : 0
  )
}
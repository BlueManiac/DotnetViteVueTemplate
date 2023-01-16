export { }
declare global {
  interface Array<T> {
    orderBy(lambda: (item: T) => any, reverse?: boolean)
  }
}

Array.prototype.orderBy = function (lambda, reverse = false) {
  const sortedList = this.slice(0).sort((a, b) =>
    lambda(a) > lambda(b) ? 1 : lambda(a) < lambda(b) ? -1 : 0
  )

  return reverse
    ? sortedList.reverse()
    : sortedList
}
/** Extract a human-readable message from an RTK Query / axios error. */
export const errMsg = (e: any): string =>
  e?.data?.message || e?.data?.title || e?.message || 'Something went wrong';

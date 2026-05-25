export function getApiError(err, fallback = 'Có lỗi xảy ra') {
  const status = err?.response?.status;
  if (status === 403) return 'Bạn không có quyền thực hiện thao tác này (cần IT Admin)';
  if (status === 401) return 'Phiên đăng nhập hết hạn, vui lòng đăng nhập lại';
  return err?.response?.data?.error?.message
    || err?.response?.data?.message
    || (typeof err?.response?.data === 'string' ? err.response.data : null)
    || err?.message
    || fallback;
}

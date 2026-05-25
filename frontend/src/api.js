import axios from "axios";

const API = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5238/api/v1"
});

API.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

API.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401 && !err.config?.url?.includes("/auth/login")) {
      localStorage.clear();
      if (!window.location.pathname.includes("/login")) {
        window.location.href = "/login";
      }
    }
    return Promise.reject(err);
  }
);

export const login = (data) => API.post("/auth/login", data);
export const seedAdmin = () => API.post("/auth/seed");

export const getDashboardSummary = () => API.get("/dashboard/summary");

export const getDevices = (keyword = "", status = "") =>
  API.get(`/devices?keyword=${encodeURIComponent(keyword)}&status=${encodeURIComponent(status)}`);
export const getDeviceById = (id) => API.get(`/devices/${id}`);
export const createDevice = (data) => API.post("/devices", data);
export const updateDevice = (id, data) => API.put(`/devices/${id}`, data);
export const disposeDevice = (id) => API.patch(`/devices/${id}/dispose`);

export const getUsers = (includeInactive = false) =>
  API.get(`/users?includeInactive=${includeInactive}`);
export const createUser = (data) => API.post("/users", data);
export const updateUser = (id, data) => API.put(`/users/${id}`, data);

export const getRequests = (type = "", status = "") =>
  API.get(`/requests?type=${type}&status=${status}`);
export const createAllocationRequest = (data) => API.post("/requests/allocation", data);
export const createAllocationRequestSelf = (data) => API.post("/requests/allocation/self", data);
export const createRecoveryRequest = (data) => API.post("/requests/recovery", data);
export const approveAllocation = (id, data) => API.patch(`/requests/${id}/approve-allocation`, data);
export const confirmRecovery = (id, data) => API.patch(`/requests/${id}/confirm-recovery`, data);
export const rejectRequest = (id, data) => API.patch(`/requests/${id}/reject`, data);

export const getDepreciations = () => API.get("/depreciations");
export const setupDepreciation = (data) => API.post("/depreciations", data);

export const getAllocationHistory = () => API.get("/history/allocations");
export const getRecoveryHistory = () => API.get("/history/recoveries");
export const getActivityHistory = () => API.get("/history/activity");

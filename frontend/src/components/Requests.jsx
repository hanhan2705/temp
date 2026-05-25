import { useEffect, useState, useCallback } from 'react';
import {
  getRequests,
  approveAllocation,
  confirmRecovery,
  rejectRequest,
  createAllocationRequest,
  createAllocationRequestSelf,
  createRecoveryRequest,
  getDevices,
  getUsers,
  getAllocationHistory,
} from '../api';
import { getStoredUser } from '../utils/user';
import { PERMISSIONS } from '../utils/permissions';
import { useToast } from './Toast';
import { getApiError } from '../utils/apiError';

export default function Requests() {
  const user = getStoredUser();
  const role = user?.role;
  const isAdmin = PERMISSIONS.approveRequests(role);
  const isEmployee = role === 'EMPLOYEE';
  const canRecovery = PERMISSIONS.sendRecovery(role);

  const [requests, setRequests] = useState([]);
  const [users, setUsers] = useState([]);
  const [assignedDevices, setAssignedDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showNewModal, setShowNewModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(null);
  const [availableDevices, setAvailableDevices] = useState([]);
  const [approveDeviceId, setApproveDeviceId] = useState('');
  const [requesterAllocations, setRequesterAllocations] = useState([]);
  const [showRejectModal, setShowRejectModal] = useState(null);
  const [rejectReason, setRejectReason] = useState('');
  const [showRecoveryModal, setShowRecoveryModal] = useState(null);
  const [deviceCondition, setDeviceCondition] = useState('GOOD');
  const [newForm, setNewForm] = useState({
    requestType: 'ALLOCATION',
    targetEmployeeId: '',
    deviceId: '',
    reason: '',
  });
  const { showToast } = useToast();

  const fetchRequests = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getRequests(typeFilter, statusFilter);
      setRequests(res.data.data || []);
    } catch (err) {
      showToast(getApiError(err, 'Không tải được yêu cầu'), 'error');
      setRequests([]);
    } finally {
      setLoading(false);
    }
  }, [typeFilter, statusFilter, showToast]);

  useEffect(() => {
    fetchRequests();
  }, [fetchRequests]);

  useEffect(() => {
    if (!isEmployee && showNewModal) {
      getUsers().then((res) => setUsers(res.data.data || [])).catch(() => { });
    }
  }, [showNewModal, isEmployee]);

  useEffect(() => {
    if (newForm.requestType === 'RECOVERY' && newForm.targetEmployeeId) {
      getDevices('', 'ASSIGNED').then((res) => {
        const list = (res.data.data || []).filter(
          (d) => d.assignedUserId === Number(newForm.targetEmployeeId)
        );
        setAssignedDevices(list);
      });
    }
  }, [newForm.targetEmployeeId, newForm.requestType]);

  const openApprove = async (req) => {
    try {
      if (req.requestType === 'ALLOCATION') {
        const res = await getDevices('', 'AVAILABLE');
        setAvailableDevices(res.data.data || []);
        setApproveDeviceId('');
        setRequesterAllocations([]);
        setShowApproveModal(req);

        getAllocationHistory()
          .then((histRes) => {
            const userHist = (histRes.data.data || []).filter(
              (h) => h.userId === req.userId || h.userId === req.user?.id
            );
            setRequesterAllocations(userHist);
          })
          .catch(() => { });
      } else {
        setShowRecoveryModal(req);
        setDeviceCondition('GOOD');
      }
    } catch (err) {
      showToast(getApiError(err, 'Thao tác thất bại'), 'error');
    }
  };

  const handleApproveSubmit = async () => {
    if (!showApproveModal || !approveDeviceId) {
      showToast('Vui lòng chọn thiết bị', 'warning');
      return;
    }
    try {
      await approveAllocation(showApproveModal.id, { deviceId: Number(approveDeviceId), note: 'Duyệt cấp phát' });
      setShowApproveModal(null);
      showToast('Duyệt và cấp phát thành công!', 'success');
      await fetchRequests();
    } catch (err) {
      showToast(getApiError(err, 'Duyệt thất bại'), 'error');
    }
  };

  const handleRecoveryConfirm = async () => {
    try {
      await confirmRecovery(showRecoveryModal.id, {
        deviceCondition,
        note: 'Xác nhận thu hồi',
      });

      showToast('Thu hồi thiết bị thành công!', 'success');

      setShowRecoveryModal(null);

      await fetchRequests();
    } catch (err) {
      showToast(getApiError(err, 'Thu hồi thất bại'), 'error');
    }
  };

  // const handleReject = async (id) => {
  //   const reason = window.prompt('Nhập lý do từ chối:');
  //   if (reason === null) return;
  //   const cleanReason = reason.trim();
  //   if (!cleanReason) {
  //     showToast('Vui lòng nhập lý do từ chối', 'warning');
  //     return;
  //   }
  //   try {
  //     await rejectRequest(id, { reason: cleanReason });
  //     showToast('Đã từ chối yêu cầu', 'warning');
  //     await fetchRequests();
  //   } catch (err) {
  //     showToast(getApiError(err, 'Từ chối thất bại'), 'error');
  //   }
  // };
  const handleReject = async () => {
    if (!rejectReason.trim()) {
      showToast('Vui lòng nhập lý do từ chối', 'warning');
      return;
    }

    try {
      await rejectRequest(showRejectModal.id, {
        reason: rejectReason.trim(),
      });

      showToast('Đã từ chối yêu cầu', 'warning');

      setShowRejectModal(null);
      setRejectReason('');

      await fetchRequests();
    } catch (err) {
      showToast(getApiError(err, 'Từ chối thất bại'), 'error');
    }
  };

  const handleSubmitRequest = async () => {
    if (!newForm.reason.trim()) {
      showToast('Vui lòng nhập lý do', 'warning');
      return;
    }
    try {
      if (isEmployee) {
        await createAllocationRequestSelf({ reason: newForm.reason });
      } else if (newForm.requestType === 'ALLOCATION') {
        if (!newForm.targetEmployeeId) {
          showToast('Chọn nhân viên nhận cấp phát', 'warning');
          return;
        }
        await createAllocationRequest({
          targetEmployeeId: Number(newForm.targetEmployeeId),
          reason: newForm.reason,
        });
      } else {
        if (!newForm.targetEmployeeId || !newForm.deviceId) {
          showToast('Chọn nhân viên và thiết bị thu hồi', 'warning');
          return;
        }
        await createRecoveryRequest({
          targetEmployeeId: Number(newForm.targetEmployeeId),
          deviceId: Number(newForm.deviceId),
          reason: newForm.reason,
        });
      }
      setShowNewModal(false);
      setNewForm({ requestType: 'ALLOCATION', targetEmployeeId: '', deviceId: '', reason: '' });
      showToast('Gửi yêu cầu thành công!', 'success');
      await fetchRequests();
    } catch (err) {
      showToast(getApiError(err, 'Gửi yêu cầu thất bại'), 'error');
    }
  };

  const requestTypes = isEmployee
    ? [{ value: 'ALLOCATION', label: 'Cấp phát thiết bị' }]
    : [
      { value: 'ALLOCATION', label: 'Cấp phát thiết bị' },
      ...(canRecovery ? [{ value: 'RECOVERY', label: 'Thu hồi thiết bị' }] : []),
    ];

  return (
    <div className="page active" id="page-requests">
      <div className="action-bar">
        <div className="filter-bar">
          <select className="filter-select" value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)}>
            <option value="">Tất cả loại</option>
            <option value="ALLOCATION">Cấp phát</option>
            {canRecovery && <option value="RECOVERY">Thu hồi</option>}
          </select>
          <select className="filter-select" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">Tất cả trạng thái</option>
            <option value="PENDING">Chờ duyệt</option>
            <option value="APPROVED">Đã duyệt</option>
            <option value="REJECTED">Từ chối</option>
          </select>
        </div>
        <button type="button" className="btn btn-primary" onClick={() => setShowNewModal(true)}>
          <i className="ti ti-plus" /> Gửi yêu cầu
        </button>
      </div>

      {isEmployee && (
        <div style={{ marginBottom: '14px', fontSize: '13px', color: 'var(--text3)' }}>
          Bạn chỉ xem được yêu cầu do mình gửi.
        </div>
      )}

      <div id="requestsList">
        {loading ? (
          <div style={{ padding: '20px', color: 'var(--text3)' }}>Đang tải...</div>
        ) : (
          requests.map((r) => (
            <div
              key={r.id}
              className={`request-card ${r.status === 'PENDING' ? 'pending' : r.status === 'APPROVED' ? 'approved' : 'rejected'}`}
            >
              <div className="request-card-header">
                <div>
                  <div style={{ fontWeight: 600, fontSize: '14px' }}>
                    YC-{String(r.id).padStart(4, '0')} — {r.requestType === 'ALLOCATION' ? 'Cấp phát' : 'Thu hồi'}
                  </div>
                  <div className="request-card-meta">
                    <span><i className="ti ti-user" style={{ fontSize: '12px' }} /> {r.user?.fullName}</span>
                    <span>
                      <i className="ti ti-calendar" style={{ fontSize: '12px' }} />{' '}
                      {r.createdAt ? new Date(r.createdAt).toLocaleDateString('vi-VN') : '—'}
                    </span>
                    {r.createdByUser && r.createdByUser.id !== r.user?.id && (
                      <span>
                        <i className="ti ti-user-check" style={{ fontSize: '12px' }} /> Gửi bởi: {r.createdByUser.fullName}
                      </span>
                    )}
                  </div>
                </div>
                <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                  <span className={`badge ${r.requestType === 'ALLOCATION' ? 'badge-info' : 'badge-danger'}`}>
                    {r.requestType === 'ALLOCATION' ? 'Cấp phát' : 'Thu hồi'}
                  </span>
                  <span
                    className={`badge ${r.status === 'PENDING' ? 'badge-warning' : r.status === 'APPROVED' ? 'badge-success' : 'badge-danger'
                      }`}
                  >
                    {r.status === 'PENDING' ? 'Chờ duyệt' : r.status === 'APPROVED' ? 'Đã duyệt' : 'Từ chối'}
                  </span>
                </div>
              </div>
              <div style={{ fontSize: '13.5px', color: 'var(--text2)' }}>
                Lý do: {r.reason || r.requestDevices?.[0]?.note || '—'}
              </div>
              {r.status === 'PENDING' && isAdmin && (
                <div className="request-card-actions">
                  <button type="button" className="btn btn-primary btn-sm" onClick={() => openApprove(r)}>
                    <i className="ti ti-check" />{' '}
                    {r.requestType === 'ALLOCATION' ? 'Duyệt & Cấp phát' : 'Xác nhận thu hồi'}
                  </button>
                  <button
                    type="button"
                    className="btn btn-danger btn-sm"
                    onClick={() => {
                      setShowRejectModal(r);
                      setRejectReason('');
                    }}
                  >
                    <i className="ti ti-x" /> Từ chối
                  </button>
                </div>
              )}
            </div>
          ))
        )}
        {!loading && requests.length === 0 && (
          <div className="empty-state">
            <div className="empty-text">Không có yêu cầu</div>
          </div>
        )}
      </div>

      {showNewModal && (
        <div className="modal-overlay open" onClick={(e) => e.target === e.currentTarget && setShowNewModal(false)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">Gửi yêu cầu mới</div>
              <div className="modal-close" onClick={() => setShowNewModal(false)}><i className="ti ti-x" /></div>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <div className="form-label">Loại yêu cầu *</div>
                <select
                  className="form-select"
                  value={newForm.requestType}
                  onChange={(e) => setNewForm({ ...newForm, requestType: e.target.value, deviceId: '' })}
                >
                  {requestTypes.map((t) => (
                    <option key={t.value} value={t.value}>{t.label}</option>
                  ))}
                </select>
              </div>
              {!isEmployee && (
                <div className="form-group">
                  <div className="form-label">Nhân viên liên quan *</div>
                  <select
                    className="form-select"
                    value={newForm.targetEmployeeId}
                    onChange={(e) => setNewForm({ ...newForm, targetEmployeeId: e.target.value, deviceId: '' })}
                  >
                    <option value="">-- Chọn nhân viên --</option>
                    {users
                      .filter((u) => u.role === 'EMPLOYEE')
                      .map((u) => (
                        <option key={u.id} value={u.id}>
                          {u.fullName} ({u.email})
                        </option>
                      ))}
                  </select>
                </div>
              )}
              {newForm.requestType === 'RECOVERY' && (
                <div className="form-group">
                  <div className="form-label">Thiết bị thu hồi *</div>
                  <select
                    className="form-select"
                    value={newForm.deviceId}
                    onChange={(e) => setNewForm({ ...newForm, deviceId: e.target.value })}
                  >
                    <option value="">-- Chọn thiết bị --</option>
                    {assignedDevices.map((d) => (
                      <option key={d.id} value={d.id}>
                        TB-{String(d.id).padStart(3, '0')} — {d.name}
                      </option>
                    ))}
                  </select>
                </div>
              )}
              <div className="form-group">
                <div className="form-label">Lý do *</div>
                <textarea
                  className="form-textarea"
                  value={newForm.reason}
                  onChange={(e) => setNewForm({ ...newForm, reason: e.target.value })}
                  placeholder="Mô tả lý do..."
                />
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setShowNewModal(false)}>Hủy</button>
              <button type="button" className="btn btn-primary" onClick={handleSubmitRequest}>
                <i className="ti ti-send" /> Gửi yêu cầu
              </button>
            </div>
          </div>
        </div>
      )}

      {showApproveModal && (
        <div className="modal-overlay open" onClick={(e) => e.target === e.currentTarget && setShowApproveModal(null)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">Duyệt & Cấp phát</div>
              <div className="modal-close" onClick={() => setShowApproveModal(null)}><i className="ti ti-x" /></div>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <div className="form-label">Chọn thiết bị *</div>
                <select className="form-select" value={approveDeviceId} onChange={(e) => setApproveDeviceId(e.target.value)}>
                  <option value="">-- Chọn --</option>
                  {availableDevices.map((d) => (
                    <option key={d.id} value={d.id}>
                      TB-{String(d.id).padStart(3, '0')} — {d.name}
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ marginTop: '16px', borderTop: '1px solid var(--border)', paddingTop: '16px' }}>
                <div className="form-label" style={{ fontWeight: 600 }}>Lịch sử cấp phát của nhân viên này:</div>
                {requesterAllocations.length === 0 ? (
                  <div style={{ fontSize: '12.5px', color: 'var(--text3)', padding: '6px 0' }}>Chưa từng có cấp phát trước đây.</div>
                ) : (
                  <div style={{ maxHeight: '120px', overflowY: 'auto', fontSize: '12.5px', background: 'var(--bg3)', borderRadius: '6px', padding: '10px', marginTop: '6px' }}>
                    {requesterAllocations.map((h) => (
                      <div key={h.id} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px', borderBottom: '1px dashed var(--border)', paddingBottom: '4px' }}>
                        <span>{h.deviceName} ({h.deviceCode})</span>
                        <span style={{ color: 'var(--text3)' }}>{new Date(h.date).toLocaleDateString('vi-VN')}</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setShowApproveModal(null)}>Hủy</button>
              <button type="button" className="btn btn-primary" onClick={handleApproveSubmit}>Xác nhận</button>
            </div>
          </div>
        </div>
      )}
      {
        showRejectModal && (
          <div
            className="modal-overlay open"
            onClick={(e) =>
              e.target === e.currentTarget && setShowRejectModal(null)
            }
          >
            <div className="modal">
              <div className="modal-header">
                <div className="modal-title">Từ chối yêu cầu</div>

                <div
                  className="modal-close"
                  onClick={() => setShowRejectModal(null)}
                >
                  <i className="ti ti-x" />
                </div>
              </div>

              <div className="modal-body">
                <div className="form-group">
                  <div className="form-label">
                    Lý do từ chối *
                  </div>

                  <textarea
                    className="form-textarea"
                    value={rejectReason}
                    onChange={(e) =>
                      setRejectReason(e.target.value)
                    }
                    placeholder="Nhập lý do từ chối..."
                    style={{
                      minHeight: '120px',
                      resize: 'none'
                    }}
                  />
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowRejectModal(null)}
                >
                  Hủy
                </button>

                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={handleReject}
                >
                  <i className="ti ti-x" />
                  Từ chối yêu cầu
                </button>
              </div>
            </div>
          </div>
        )
      }

      {showRecoveryModal && (
        <div
          className="modal-overlay open"
          onClick={(e) =>
            e.target === e.currentTarget &&
            setShowRecoveryModal(null)
          }
        >
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">
                Xác nhận thu hồi
              </div>

              <div
                className="modal-close"
                onClick={() => setShowRecoveryModal(null)}
              >
                <i className="ti ti-x" />
              </div>
            </div>

            <div className="modal-body">
              <div className="form-group">
                <div className="form-label">
                  Tình trạng thiết bị *
                </div>

                <select
                  className="form-select"
                  value={deviceCondition}
                  onChange={(e) =>
                    setDeviceCondition(e.target.value)
                  }
                >
                  <option value="GOOD">
                    GOOD - Hoạt động tốt
                  </option>

                  <option value="OK">
                    OK - Bình thường
                  </option>

                  <option value="BAD">
                    BAD - Hỏng / lỗi
                  </option>
                </select>
              </div>
            </div>

            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setShowRecoveryModal(null)}
              >
                Hủy
              </button>

              <button
                type="button"
                className="btn btn-primary"
                onClick={handleRecoveryConfirm}
              >
                <i className="ti ti-check" />
                Xác nhận thu hồi
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

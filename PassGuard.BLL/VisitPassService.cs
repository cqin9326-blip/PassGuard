using System.Collections.Generic;
using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class VisitPassService
    {
        private readonly VisitPassRepository _repo;
        private readonly PassCodeService _passCodeService;

        public VisitPassService(VisitPassRepository repo, PassCodeService passCodeService)
        {
            _repo = repo;
            _passCodeService = passCodeService;
        }

        public List<VisitPass> GetAllWithDetails()
        {
            List<VisitPass> visitPasses = _repo.GetAllWithDetails();
            NormalizeStatuses(visitPasses);
            return visitPasses;
        }

        public VisitPass? GetFullDetails(int id)
        {
            VisitPass? visitPass = _repo.GetFullDetails(id);

            if (visitPass != null)
            {
                NormalizeStatus(visitPass);
            }

            return visitPass;
        }

        public VisitPass? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public VisitPass? GetFirstByHomeId(int homeId)
        {
            return _repo.GetFirstByHomeId(homeId);
        }

        public List<VisitPass> GetByCreatedByUserId(string createdByUserId)
        {
            List<VisitPass> visitPasses = _repo.GetByCreatedByUserId(createdByUserId);
            NormalizeStatuses(visitPasses);
            return visitPasses;
        }

        public void Add(VisitPass visitPass)
        {
            _repo.Add(visitPass);
        }

        public void Update(VisitPass visitPass)
        {
            _repo.Update(visitPass);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public string GeneratePlainCode()
        {
            return _passCodeService.GeneratePlainCode();
        }

        public string HashCode(string plainCode)
        {
            return _passCodeService.HashCode(plainCode);
        }

        public bool VerifyCode(VisitPass visitPass, string plainCode)
        {
            return _passCodeService.VerifyCode(plainCode, visitPass.CodeHash);
        }

        public string NormalizeStatus(VisitPass visitPass)
        {
            if (!PassStatuses.Allowed.Contains(visitPass.Status))
            {
                visitPass.Status = PassStatuses.Active;
            }

            string calculatedStatus = _passCodeService.CalculateStatus(visitPass);

            if (!string.Equals(visitPass.Status, calculatedStatus, StringComparison.Ordinal))
            {
                visitPass.Status = calculatedStatus;
                _repo.Update(visitPass);
            }

            return visitPass.Status;
        }

        public bool CanBeAccepted(VisitPass visitPass)
        {
            string status = NormalizeStatus(visitPass);
            return string.Equals(status, PassStatuses.Active, StringComparison.Ordinal);
        }

        public void Revoke(VisitPass visitPass)
        {
            NormalizeStatus(visitPass);

            if (string.Equals(visitPass.Status, PassStatuses.Used, StringComparison.Ordinal) ||
                string.Equals(visitPass.Status, PassStatuses.Expired, StringComparison.Ordinal))
            {
                return;
            }

            visitPass.Status = PassStatuses.Revoked;
            _repo.Update(visitPass);
        }

        public bool HasActivePassForVisitorAndHome(int visitorId, int homeId, int? ignoreVisitPassId = null)
        {
            return _repo.HasActivePassForVisitorAndHome(visitorId, homeId, ignoreVisitPassId);
        }

        private void NormalizeStatuses(IEnumerable<VisitPass> visitPasses)
        {
            foreach (VisitPass visitPass in visitPasses)
            {
                NormalizeStatus(visitPass);
            }
        }
    }
}

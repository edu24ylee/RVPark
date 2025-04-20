using ApplicationCore.Models;
using ApplicationCore.ViewModels;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Guests
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public UpsertModel(UnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [BindProperty]
        public GuestViewModel GuestVM { get; set; } = new();

        [BindProperty]
        public Rv Rv { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(int? id, string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (id == null || id == 0)
            {
                GuestVM = new GuestViewModel();
                Rv = new Rv();
                return Page();
            }

            var guest = _unitOfWork.Guest.Get(g => g.GuestId == id, includes: "User,DodAffiliation,Rvs");
            if (guest == null || guest.User == null)
                return NotFound();

            var primaryRv = guest.Rvs?.FirstOrDefault();

            GuestVM = new GuestViewModel
            {
                GuestId = guest.GuestId,
                UserId = guest.User.UserId,
                FirstName = guest.User.FirstName,
                LastName = guest.User.LastName,
                Email = guest.User.Email,
                Phone = guest.User.Phone,
                DodId = guest.DodId == 0 ? null : guest.DodId,
                DodBranch = guest.DodAffiliation?.Branch ?? string.Empty,
                DodStatus = guest.DodAffiliation?.Status ?? string.Empty,
                DodRank = guest.DodAffiliation?.Rank ?? string.Empty
            };

            Rv = primaryRv ?? new Rv { GuestId = guest.GuestId };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ReturnUrl ??= Request.Query["returnUrl"];
                return Page();
            }

            bool hasAllDodInfo =
                !string.IsNullOrWhiteSpace(GuestVM.DodBranch) &&
                !string.IsNullOrWhiteSpace(GuestVM.DodStatus) &&
                !string.IsNullOrWhiteSpace(GuestVM.DodRank) &&
                GuestVM.DodId.HasValue && GuestVM.DodId > 0;

            if (GuestVM.GuestId == 0)
            {
                var identityUser = new IdentityUser
                {
                    UserName = GuestVM.Email,
                    Email = GuestVM.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(identityUser, "Guest123!");
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return Page();
                }

                await _userManager.AddToRoleAsync(identityUser, SD.GuestRole);

                var user = new User
                {
                    FirstName = GuestVM.FirstName.Trim(),
                    LastName = GuestVM.LastName.Trim(),
                    Email = GuestVM.Email.Trim(),
                    Phone = GuestVM.Phone?.Trim(),
                    IsActive = true,
                    IdentityUserId = identityUser.Id
                };

                _unitOfWork.User.Add(user);
                await _unitOfWork.CommitAsync();

                var guest = new Guest
                {
                    UserId = user.UserId,
                    DodId = GuestVM.DodId ?? 0
                };

                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                if (hasAllDodInfo)
                {
                    _unitOfWork.DodAffiliation.Add(new DodAffiliation
                    {
                        GuestId = guest.GuestId,
                        Branch = GuestVM.DodBranch,
                        Status = GuestVM.DodStatus,
                        Rank = GuestVM.DodRank
                    });
                }

                Rv.GuestId = guest.GuestId;
                _unitOfWork.Rv.Add(Rv);

                await _unitOfWork.CommitAsync();

                return RedirectToPage("Index");
            }
            else
            {
                // === UPDATE GUEST ===
                var existingGuest = _unitOfWork.Guest.Get(g => g.GuestId == GuestVM.GuestId, includes: "User,DodAffiliation,Rvs");
                if (existingGuest == null || existingGuest.User == null)
                    return NotFound();

                existingGuest.User.FirstName = GuestVM.FirstName.Trim();
                existingGuest.User.LastName = GuestVM.LastName.Trim();
                existingGuest.User.Email = GuestVM.Email.Trim();
                existingGuest.User.Phone = GuestVM.Phone?.Trim();
                existingGuest.DodId = GuestVM.DodId ?? 0;

                var affiliation = _unitOfWork.DodAffiliation.Get(a => a.GuestId == GuestVM.GuestId);

                if (hasAllDodInfo)
                {
                    if (affiliation != null)
                    {
                        affiliation.Branch = GuestVM.DodBranch;
                        affiliation.Status = GuestVM.DodStatus;
                        affiliation.Rank = GuestVM.DodRank;
                        _unitOfWork.DodAffiliation.Update(affiliation);
                    }
                    else
                    {
                        _unitOfWork.DodAffiliation.Add(new DodAffiliation
                        {
                            GuestId = GuestVM.GuestId,
                            Branch = GuestVM.DodBranch,
                            Status = GuestVM.DodStatus,
                            Rank = GuestVM.DodRank
                        });
                    }
                }
                else if (affiliation != null)
                {
                    _unitOfWork.DodAffiliation.Delete(affiliation);
                }

                _unitOfWork.User.Update(existingGuest.User);
                _unitOfWork.Guest.Update(existingGuest);

                var primaryRv = existingGuest.Rvs?.FirstOrDefault();
                if (primaryRv != null)
                {
                    primaryRv.Make = Rv.Make;
                    primaryRv.Model = Rv.Model;
                    primaryRv.LicensePlate = Rv.LicensePlate;
                    primaryRv.Length = Rv.Length;
                    primaryRv.Description = Rv.Description;

                    _unitOfWork.Rv.Update(primaryRv);
                }
                else
                {
                    Rv.GuestId = existingGuest.GuestId;
                    _unitOfWork.Rv.Add(Rv);
                }

                await _unitOfWork.CommitAsync();
                return RedirectToPage("Index");
            }
        }
    }
}

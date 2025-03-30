namespace Infrastructure.Utilities
{
    public static class SD
    {
        // Roles
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string GuestRole = "Guest";
        public const string MaintenanceRole = "Maintenance";

        // Reservation Status
        public const string StatusPending = "Pending";
        public const string StatusActive = "Active";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";


        // Trailer size categories (optional usage)
        public const decimal SmallRvMaxLength = 20.0m;
        public const decimal MediumRvMaxLength = 30.0m;
        public const decimal LargeRvMaxLength = 40.0m;

        // Image Paths (if you use uploads)
        public const string RvImagePath = @"\images\rv\";
        public const string GuestImagePath = @"\images\guests\";
        public const string LotImagePath = @"\images\lots\";

        // Default Passwords (only if needed for seeding)
        public const string DefaultPassword = "RvPark123*";

        // Admin Email
        public const string DefaultAdminEmail = "admin@rvpark.com";
    }
}

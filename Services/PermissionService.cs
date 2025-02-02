namespace FinancialStrategyCalculator.Services
{
    public class PermissionService
    {
        public static async Task RequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (status != PermissionStatus.Granted)
            {
                var readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status != PermissionStatus.Granted)
            {
                var writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            // Optionally, check the status again and proceed accordingly.
        }
    }
}

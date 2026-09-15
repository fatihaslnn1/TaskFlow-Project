public class PermissionService
{
    // Sub-task yönetme yetkisi: Sadece PM ve Developer (ve Admin)
    public static bool CanManageSubTasks(string roleName)
    {
        return roleName == "Admin" || roleName == "PM" || roleName == "Developer";
    }

    // Due Date belirleme / değiştirme: Sadece PM (ve Admin)
    public static bool CanManageDueDate(string roleName)
    {
        return roleName == "Admin" || roleName == "PM";
    }

    // Attachment yükleme yetkisi: 
    // Müşteri ise sadece kendi açtığı işe (ReporterId == userId), diğer roller her yere yükleyebilir.
    public static bool CanUploadAttachment(string roleName, int currentUserId, int issueReporterId)
    {
        if (roleName == "Admin" || roleName == "PM" || roleName == "Developer" || roleName == "Reporter")
            return true;

        if (roleName == "Customer")
            return currentUserId == issueReporterId;

        return false;
    }
}
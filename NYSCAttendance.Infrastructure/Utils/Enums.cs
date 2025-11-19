using System.ComponentModel;

namespace NYSCAttendance.Infrastructure.Utils;

public enum UserTypeEnum
{
    Admin = 1,
    CorpsMemeber,
    SuperAdmin
}

public enum BatchEnum
{
    [Description("Not Given")]
    NOTGIVEN = 0,

    [Description("Batch A Stream 1")]
    A1,

    [Description("Batch A Stream 2")]
    A2,

    [Description("Batch B Stream 1")]
    B1,

    [Description("Batch A Stream 2")]
    B2,

    [Description("Batch C Stream 1")]
    C1,

    [Description("Batch C Stream 2")]
    C2
}

public enum OTPStatusEnum
{
    Active,
    Used
}

public enum CDSEnum
{
    [Description("Special CDS")]
    SpecialCDS = 1,

    [Description("Editorial and Publicity CDS")]
    Editorial,

    [Description("Environmental Protection and Sanitation CDS")]
    Environmental,

    [Description("Charity and Social Welfare CDS")]
    Charity,

    [Description("Education Development CDS")]
    Education,

    [Description("Cultural and Tourism CDS")]
    Culture,

    [Description("ICT and Digital Literacy CDS")]
    ICT,

    [Description("Community Development and Special Projects CDS")]
    CommunityDevelopment,

    [Description("Agriculture and Agro-Allied CDS")]
    Agriculture,

    [Description("Reproductive Health and Family Planning CDS")]
    ReproductiveHealth,

    [Description("Others")]
    Others
}

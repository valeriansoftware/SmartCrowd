using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Scheduler;

public static class ExampleSchedules
{
    public static List<ScheduleEntry> CreateFarmerSchedule()
    {
        return new List<ScheduleEntry>
        {
            new ScheduleEntry(new TimeSpan(6, 0, 0), "EatBreakfast", "table_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(10)
            },
            
            new ScheduleEntry(new TimeSpan(7, 0, 0), "ChopWood", "tree_01")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(9, 0, 0), "ChopWood", "tree_01")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(10, 0, 0), "EatLunch", "table_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(15)
            },
            
            new ScheduleEntry(new TimeSpan(11, 0, 0), "ChopWood", "tree_02")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(13, 0, 0), "ChopWood", "tree_02")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(14, 0, 0), "Trade", "trader_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(30)
            },
            
            new ScheduleEntry(new TimeSpan(16, 0, 0), "ChopWood", "tree_03")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(18, 0, 0), "ChopWood", "tree_03")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(19, 0, 0), "EatDinner", "table_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(20)
            },
            
            new ScheduleEntry(new TimeSpan(22, 0, 0), "Rest", "bed_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(10)
            }
        };
    }
    
    public static List<ScheduleEntry> CreateMerchantSchedule()
    {
        return new List<ScheduleEntry>
        {
            new ScheduleEntry(new TimeSpan(8, 0, 0), "OpenShop", "shop_door")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(9, 0, 0), "Trade", "counter_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            },
            
            new ScheduleEntry(new TimeSpan(12, 0, 0), "EatLunch", "table_02")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(15)
            },
            
            new ScheduleEntry(new TimeSpan(13, 0, 0), "Trade", "counter_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            },
            
            new ScheduleEntry(new TimeSpan(18, 0, 0), "CloseShop", "shop_door")
            {
                IsInterruptible = false,
                RetryIfBusy = false
            },
            
            new ScheduleEntry(new TimeSpan(19, 0, 0), "EatDinner", "table_02")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(20)
            }
        };
    }
    
    public static List<ScheduleEntry> CreateGuardSchedule()
    {
        return new List<ScheduleEntry>
        {
            new ScheduleEntry(new TimeSpan(6, 0, 0), "Patrol", "gate_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            },
            
            new ScheduleEntry(new TimeSpan(8, 0, 0), "EatBreakfast", "table_03")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(10)
            },
            
            new ScheduleEntry(new TimeSpan(9, 0, 0), "Patrol", "tower_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            },
            
            new ScheduleEntry(new TimeSpan(12, 0, 0), "EatLunch", "table_03")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(15)
            },
            
            new ScheduleEntry(new TimeSpan(13, 0, 0), "Patrol", "wall_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            },
            
            new ScheduleEntry(new TimeSpan(18, 0, 0), "EatDinner", "table_03")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(20)
            },
            
            new ScheduleEntry(new TimeSpan(19, 0, 0), "Patrol", "gate_01")
            {
                IsInterruptible = true,
                RetryIfBusy = true,
                RetryInterval = TimeSpan.FromMinutes(5)
            }
        };
    }
} 
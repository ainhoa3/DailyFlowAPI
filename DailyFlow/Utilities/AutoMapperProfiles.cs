
using AutoMapper;
using DailyFlow.Entities;
using DailyFlow.DTOs;

namespace DailyFlow.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        private readonly int descriptonPreviewMaxLength;

        public int GetPreviewMax()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            int previewMax = configuration.GetValue<int>("PreviewMax");
            return previewMax;
        }
        public string DescriptionPreview(string description, int max)
        {
            
            if(description.Length > max) 
            {
                description = description.Substring(0, max);
            }

            return description;
        }
        public AutoMapperProfiles()
        {
            // tasks
            descriptonPreviewMaxLength = GetPreviewMax();

            CreateMap<_Task, TaskDTO>()
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DeuDate))
                .ForMember(dto => dto.Priority, config => config
                .MapFrom(task => task.CalcualatePriority()));
            CreateMap<TaskDTO, _Task>()
                .ForMember(dest => dest.DeuDate, opt => opt.MapFrom(src => src.DueDate));

            CreateMap<_Task, TaskPreviewDTO>()
                .ForMember(dto => dto.Description, config => config
                    .MapFrom(task => DescriptionPreview(task.Description, descriptonPreviewMaxLength)))
                .ForMember(dto => dto.Priority, config => config
                    .MapFrom(task => task.CalcualatePriority()));


            CreateMap<_Task, TaskCreatingDTO>()
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DeuDate));
            CreateMap<TaskCreatingDTO, _Task>()
                .ForMember(dest => dest.DeuDate, opt => opt.MapFrom(src => src.DueDate));

            CreateMap<TaskUpdatingDTO, _Task>()
                .ForMember(dest => dest.DeuDate, opt => opt.MapFrom(src => src.DueDate));
            CreateMap<_Task, TaskUpdatingDTO>()
               .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DeuDate));

            // habits 
            CreateMap<HabitCreatingDTO, Habit>()
             .ForMember(habit => habit.LastDay, config => config.MapFrom(dto => dto.StartingDay));
            CreateMap<HabitUpdatingDTO, Habit>()
            .ForMember(habit => habit.LastDay, config => config.MapFrom(dto => dto.StartingDay));

            CreateMap<Habit, HabitCreatingDTO>().ReverseMap();
            CreateMap<HabitUpdatingDTO, Habit>().ReverseMap();
        }
    }

}

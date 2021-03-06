﻿using AutoMapper;
using dataaccesscore.Abstractions.Uow;
using Newtonsoft.Json;
using Survey.ApplicationLayer.Dtos.Models.Answers;
using Survey.ApplicationLayer.Services.Interfaces;
using Survey.Common.Enums;
using Survey.DomainModelLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.ApplicationLayer.Services
{
    public class AnswerService : IAnswerService
    {
        AnswerTypes type;
        private readonly IQuestionOptionService _questionOptionService;
        private readonly IOptionChoiceService _optionChoiceService;
        protected readonly IUowProvider UowProvider;
        protected readonly IMapper Mapper;

        private Guid respondentId;
        private BaseAnswerModel baseAnswerModel;
        private Dictionary<Type, Action> switchAnswerType;



        public AnswerService(
            IQuestionOptionService questionOptionService,
            IOptionChoiceService optionChoiceService,
            IUowProvider uowProvider,
            IMapper mapper
            )
        {
            _questionOptionService = questionOptionService;
            _optionChoiceService = optionChoiceService;
            this.UowProvider = uowProvider;
            this.Mapper = mapper;

            switchAnswerType = new Dictionary<Type, Action> {
                { typeof(TextAnswerModel), () =>
                    {
                        SaveTextAnswer((TextAnswerModel) baseAnswerModel);
                    }
                },
                { typeof(TextAreaAnswerModel), () =>
                    {
                        SaveTextAreaAnswer((TextAreaAnswerModel) baseAnswerModel);
                    }
                },
                { typeof(RadioAnswerModel), () =>
                    {
                        SaveRadioAnswer((RadioAnswerModel) baseAnswerModel);
                    }
                },
                { typeof(CheckBoxAnswerModel), () =>
                    {
                        SaveCheckBoxAnswer((CheckBoxAnswerModel) baseAnswerModel);
                    }
                },
                { typeof(DropdownAnswerModel), () =>
                    {
                        SaveDropdownAnswer((DropdownAnswerModel) baseAnswerModel);
                    }
                },
                {  typeof(GridAnswerModel), () =>
                    {
                        SaveGridRadioAnswer((GridAnswerModel) baseAnswerModel);
                    }
                },
            };
        }


        public async Task<Guid> AddAsync(Answers answer)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryAnswer = uow.GetRepository<Answers, Guid>();
                await repositoryAnswer.AddAsync(answer);
                await uow.SaveChangesAsync();
                return answer.Id;
            }
        }


        public async Task Remove(Answers answer)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryAnswer = uow.GetRepository<Answers, Guid>();
                repositoryAnswer.Remove(answer);
                uow.SaveChanges();
            }
        }


        public async Task<IEnumerable<Answers>> GetAllAsync()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryAnswer = uow.GetRepository<Answers, Guid>();
                return await repositoryAnswer.GetAllAsync();
            }
        }


        public List<BaseAnswerModel> GetTypedAnswerList(List<object> survey)
        {
            List<BaseAnswerModel> baseAnswerList = new List<BaseAnswerModel>();
            if (survey != null)
            {
                foreach (var item in survey)
                {
                    BaseAnswerModel question = GetQuestionByType(item.ToString());
                    baseAnswerList.Add(question);
                }
            }
            return baseAnswerList;
        }


        public void SaveAnswerByType(BaseAnswerModel baseAnswer, Guid id)
        {
            baseAnswerModel = baseAnswer;
            respondentId = id;
            switchAnswerType[baseAnswer.GetType()]();
        }


        public BaseAnswerModel GetQuestionByType(string baseQuestion)
        {
            var baseAnswerM = JsonConvert.DeserializeObject<BaseAnswerModel>(baseQuestion);
            if (Enum.TryParse(baseAnswerM.Type.ToString(), out type))
            {
                switch (type)
                {
                    case AnswerTypes.Textbox:
                        {
                            TextAnswerModel question = JsonConvert.DeserializeObject<TextAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                    case AnswerTypes.Textarea:
                        {
                            TextAreaAnswerModel question = JsonConvert.DeserializeObject<TextAreaAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                    case AnswerTypes.Radio:
                        {
                            RadioAnswerModel question = JsonConvert.DeserializeObject<RadioAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                    case AnswerTypes.Checkbox:
                        {
                            CheckBoxAnswerModel question = JsonConvert.DeserializeObject<CheckBoxAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                    case AnswerTypes.Dropdown:
                        {
                            DropdownAnswerModel question = JsonConvert.DeserializeObject<DropdownAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                    case AnswerTypes.GridRadio:
                        {
                            GridAnswerModel question = JsonConvert.DeserializeObject<GridAnswerModel>(baseQuestion);
                            baseAnswerM = question as BaseAnswerModel;
                            break;
                        }
                }
            }
            return baseAnswerM;
        }


        private void SaveTextAnswer(TextAnswerModel data)
        {
            if (data.Answer.Length > 0)
            {
                var optionChoice = _optionChoiceService.GetListByOptionGroupId(data.OptionGroupId).Result.FirstOrDefault();
                var questionOptionId = _questionOptionService.Add(data.Id, optionChoice.Id);

                Answers answer = new Answers()
                {
                    AnswerBoolean = false,
                    AnswerDateTime = null,
                    AnswerNumeric = null,
                    AnswerText = data.Answer,
                    RespondentId = respondentId,
                    QuestionOptionId = questionOptionId
                };
                var answerId = AddAsync(answer).Result;
            }
        }

        private void SaveTextAreaAnswer(TextAreaAnswerModel data)
        {
            if (data.Answer.Length > 0)
            {
                var optionChoice = _optionChoiceService.GetListByOptionGroupId(data.OptionGroupId).Result.FirstOrDefault();
                var questionOptionId = _questionOptionService.Add(data.Id, optionChoice.Id);

                Answers answer = new Answers()
                {
                    AnswerBoolean = false,
                    AnswerDateTime = null,
                    AnswerNumeric = null,
                    AnswerText = data.Answer,
                    RespondentId = respondentId,
                    QuestionOptionId = questionOptionId
                };
                var answerId = AddAsync(answer).Result;
            }
        }



        private void SaveRadioAnswer(RadioAnswerModel data)
        {
            if (data.Answer.Length > 0)
            {
                var questionOptionId = _questionOptionService.Add(data.Id, Guid.Parse(data.Answer));

                Answers answer = new Answers()
                {
                    AnswerBoolean = true,
                    AnswerDateTime = null,
                    AnswerNumeric = null,
                    AnswerText = null,
                    RespondentId = respondentId,
                    QuestionOptionId = questionOptionId
                };

                var answerId = AddAsync(answer).Result;
                if (!String.IsNullOrEmpty(data.AdditionalAnswer))
                {
                    AddAdditionalChoice(data);
                }
            }
        }


        public void AddAdditionalChoice(BaseAnswerModel data)
        {
            var optionChoices = _optionChoiceService.GetListByOptionGroup(data.OptionGroupId, true).Result;
            var optionChoice = optionChoices.Where(x => x.IsAdditionalChoise == true).FirstOrDefault();
            var questionOptionId = _questionOptionService.Add(data.Id, Guid.Parse(optionChoice.Id));

            Answers answer = new Answers()
            {
                AnswerBoolean = false,
                AnswerDateTime = null,
                AnswerNumeric = null,
                AnswerText = data.AdditionalAnswer,
                RespondentId = respondentId,
                QuestionOptionId = questionOptionId
            };
            var answerId = AddAsync(answer).Result;
        }



        private void SaveCheckBoxAnswer(CheckBoxAnswerModel data)
        {
            if (data.Answer.Count > 0)
            {
                foreach (var item in data.Answer)
                {
                    var questionOptionId = _questionOptionService.Add(data.Id, Guid.Parse(item));
                    Answers answer = new Answers()
                    {
                        AnswerBoolean = true,
                        AnswerDateTime = null,
                        AnswerNumeric = null,
                        AnswerText = null,
                        RespondentId = respondentId,
                        QuestionOptionId = questionOptionId
                    };
                    var answerId = AddAsync(answer).Result;
                }
                if (!String.IsNullOrEmpty(data.AdditionalAnswer))
                {
                    AddAdditionalChoice(data);
                }
            }
        }


        private void SaveDropdownAnswer(DropdownAnswerModel data)
        {
            if (data.Answer != null)
            {
                var questionOptionId = _questionOptionService.Add(data.Id, Guid.Parse(data.Answer.Id));
                Answers answer = new Answers()
                {
                    AnswerBoolean = false,
                    AnswerDateTime = null,
                    AnswerNumeric = null,
                    AnswerText = null,
                    RespondentId = respondentId,
                    QuestionOptionId = questionOptionId
                };

                var answerId = AddAsync(answer).Result;
                if (!String.IsNullOrEmpty(data.AdditionalAnswer))
                {
                    AddAdditionalChoice(data);
                }
            }
        }


        private void SaveGridRadioAnswer(GridAnswerModel data)
        {
            if (data.Answer.Count > 0)
            {
                foreach (var item in data.Answer)
                {
                    var questionOptionId = _questionOptionService.Add(Guid.Parse(item.Row.Id), Guid.Parse(item.Col.Id));

                    Answers answer = new Answers()
                    {
                        AnswerBoolean = true,
                        AnswerDateTime = null,
                        AnswerNumeric = null,
                        AnswerText = null,
                        RespondentId = respondentId,
                        QuestionOptionId = questionOptionId
                    };
                    var answerId = AddAsync(answer).Result;
                }
                if (!String.IsNullOrEmpty(data.AdditionalAnswer))
                {
                    AddAdditionalChoice(data);
                }
            }
        }
    }
}

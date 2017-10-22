using MarkupHelper.Common.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkupHelper.Common.Domain.Model;
using Serilog;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MarkupHelper.Common.Service
{
    public class MarkupRepositoryClient : IMarkupRepository
    {
        private IMarkupRepository _markupRepository;
        private readonly string _serviceEndpoint;
        private readonly ILogger _log;

        public MarkupRepositoryClient(string serviceEndpoint, ILogger log)
        {
            _serviceEndpoint = serviceEndpoint;
            _log = log;
        }

        private IMarkupRepository MarkupRepository
        {
            get
            {
                if (_markupRepository == null)
                {
                    try
                    {
                        var adress = new EndpointAddress(_serviceEndpoint);

                        var channelFactory = new ChannelFactory<IMarkupRepository>(
                            MarkupRepositoryBinding.MarkupRepository, adress);
                        _markupRepository = channelFactory.CreateChannel();
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, "Failed to create repo client");
                    }
                }
                return _markupRepository;
            }
        }

        public int CalculateUserScore(UserModel user)
        {
            try
            {
                return MarkupRepository.CalculateUserScore(user);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to call service");
                _markupRepository = null;
                throw;
            }
        }

        public string[] GetTagsList(UserModel user)
        {
            try
            {
                return MarkupRepository.GetTagsList(user);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to call service");
                _markupRepository = null;
                throw;
            }
        }

        public Group GetUnmarkedGroup(UserModel user)
        {
            try
            {
                return MarkupRepository.GetUnmarkedGroup(user);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to call service");
                _markupRepository = null;
                throw;
            }
        }

        public UserModel GetUser(string token)
        {
            try
            {
                return MarkupRepository.GetUser(token);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to call service");
                _markupRepository = null;
                throw;
            }
        }

        public bool SubmitGroupTag(UserModel user, Group group, string tag)
        {
            try
            {
                return MarkupRepository.SubmitGroupTag(user, group, tag);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to call service");
                _markupRepository = null;
                throw;
            }
        }
    }
}

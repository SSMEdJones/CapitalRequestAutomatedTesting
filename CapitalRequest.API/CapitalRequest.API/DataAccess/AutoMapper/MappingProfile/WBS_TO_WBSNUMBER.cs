using AutoMapper;
using CapitalRequest.API.Models;


namespace CapitalRequest.API.DataAccess.AutoMapper.MappingProfile
{
    public class WBS_TO_WBSNUMBER : ITypeConverter<Wbs, WbsNumber>
    {
        public WbsNumber Convert(Wbs source, WbsNumber destination, ResolutionContext context)
        {
            if (source == null)

                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                destination = new WbsNumber();

            if (source.WbsNumber == null)
            {
                return new WbsNumber();
            }

            var parseWBS = source.WbsNumber.Split("-");
            //left off here
            destination.TypeofProject = source.TypeOfProject;

            destination.Id = source.Id;
            destination.ProposalId = source.ProposalId;
            destination.TypeofProjectShortName = parseWBS[0].Trim();
            destination.CompanyCode = parseWBS[1].Trim();
            destination.CapitalPoolIdentifierShortName = parseWBS[2].Trim();
            destination.CapitalPoolShortName = parseWBS[3].Trim();
            destination.CapitalFundingYearShortName = parseWBS[4].ToString().Substring(0, 2).Trim();
            destination.UniqueIdentifier = parseWBS[4].ToString().Substring(2, 4).Trim();
            destination.Sequence = parseWBS[5].ToString().Trim();

            return destination;
        }
    }
}

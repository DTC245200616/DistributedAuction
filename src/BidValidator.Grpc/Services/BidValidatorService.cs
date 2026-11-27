using Grpc.Core;

namespace BidValidator.Grpc.Services;

public class BidValidatorService : BidValidator.BidValidatorBase
{
    public override Task<ValidateBidReply> ValidateBid(ValidateBidRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.BidderName))
            return Task.FromResult(new ValidateBidReply { IsValid = false, Message = "Tên người trả giá không được để trống." });

        if (request.Status != 0)
            return Task.FromResult(new ValidateBidReply { IsValid = false, Message = "Phiên đấu giá đã kết thúc hoặc bị hủy." });

        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (nowUnix > request.EndTimeUnix)
            return Task.FromResult(new ValidateBidReply { IsValid = false, Message = "Phiên đấu giá đã hết thời gian." });

        var minAllowed = request.CurrentPrice + request.MinStep;
        if (request.BidAmount < minAllowed)
            return Task.FromResult(new ValidateBidReply
            {
                IsValid = false,
                Message = $"Giá đặt phải lớn hơn hoặc bằng {minAllowed:N0} VNĐ (giá hiện tại + bước giá tối thiểu)."
            });

        return Task.FromResult(new ValidateBidReply { IsValid = true, Message = "Hợp lệ" });
    }
}
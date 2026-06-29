import { Injectable } from '@angular/core';
import { Loan, LoanCustomerDetail } from './loan-service';

export interface CreditScoreResult {
  score: number;
  rating: 'Excellent' | 'Good' | 'Fair' | 'Poor';
  risk: 'Low' | 'Medium' | 'High';
  eligible: boolean;
  checkedAt: Date;
  factors: string[];
}

@Injectable({
  providedIn: 'root',
})
export class CreditScoreService {
  checkScore(loan: Partial<Loan>, detail?: Partial<LoanCustomerDetail>): CreditScoreResult {
    const seed = `${loan.userId ?? ''}${loan.loanNumber ?? ''}${detail?.customerAadhaarNo ?? ''}${detail?.customerMobileNo ?? ''}`;
    const identityScore = this.getIdentityScore(detail);
    const repaymentCapacityScore = this.getRepaymentCapacityScore(loan);
    const stabilityScore = this.getStabilityScore(loan, detail);
    const deterministicOffset = this.hashSeed(seed) % 46;
    const score = this.clamp(
      Math.round(identityScore + repaymentCapacityScore + stabilityScore + deterministicOffset),
      300,
      900
    );

    return {
      score,
      rating: this.getRating(score),
      risk: this.getRisk(score),
      eligible: score >= 650,
      checkedAt: new Date(),
      factors: this.getFactors(loan, detail, score),
    };
  }

  private getIdentityScore(detail?: Partial<LoanCustomerDetail>): number {
    let score = 470;
    if (/^\d{12}$/.test(detail?.customerAadhaarNo ?? '')) score += 55;
    if (/^\d{10}$/.test(detail?.customerMobileNo ?? '')) score += 35;
    if (detail?.customerAddress?.trim()) score += 30;
    if (/^\d{6}$/.test(detail?.customerPinCode ?? '')) score += 15;
    return score;
  }

  private getRepaymentCapacityScore(loan: Partial<Loan>): number {
    const amount = Number(loan.loanAmount ?? 0);
    const tenure = Number(loan.tenure ?? 0);
    const rate = Number(loan.rate ?? 0);
    let score = 0;

    if (amount > 0 && amount <= 200000) score += 95;
    else if (amount <= 500000) score += 65;
    else if (amount <= 1000000) score += 35;
    else score += 15;

    if (tenure >= 12 && tenure <= 60) score += 45;
    else if (tenure > 60 && tenure <= 120) score += 25;

    if (rate > 0 && rate <= 18) score += 25;
    return score;
  }

  private getStabilityScore(loan: Partial<Loan>, detail?: Partial<LoanCustomerDetail>): number {
    let score = 0;
    if (loan.userName?.trim()) score += 30;
    if (detail?.customerCity?.trim() && detail?.customerState?.trim()) score += 20;
    if (detail?.guarantorName?.trim()) score += 30;
    return score;
  }

  private getFactors(
    loan: Partial<Loan>,
    detail: Partial<LoanCustomerDetail> | undefined,
    score: number
  ): string[] {
    const factors = [
      /^\d{12}$/.test(detail?.customerAadhaarNo ?? '') ? 'Aadhaar verified' : 'Aadhaar verification pending',
      /^\d{10}$/.test(detail?.customerMobileNo ?? '') ? 'Mobile number verified' : 'Mobile verification pending',
      Number(loan.loanAmount ?? 0) <= 500000 ? 'Loan amount is within standard risk range' : 'Higher loan amount increases risk',
    ];

    factors.push(score >= 650 ? 'Meets approval score threshold' : 'Below approval score threshold');
    return factors;
  }

  private getRating(score: number): CreditScoreResult['rating'] {
    if (score >= 780) return 'Excellent';
    if (score >= 700) return 'Good';
    if (score >= 650) return 'Fair';
    return 'Poor';
  }

  private getRisk(score: number): CreditScoreResult['risk'] {
    if (score >= 700) return 'Low';
    if (score >= 650) return 'Medium';
    return 'High';
  }

  private hashSeed(seed: string): number {
    return [...seed].reduce((hash, char) => (hash * 31 + char.charCodeAt(0)) >>> 0, 0);
  }

  private clamp(value: number, min: number, max: number): number {
    return Math.min(max, Math.max(min, value));
  }
}

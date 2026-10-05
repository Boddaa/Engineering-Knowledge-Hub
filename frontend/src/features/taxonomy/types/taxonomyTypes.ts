/**
 * Strongly-typed Knowledge Taxonomy, Product, and Technology contracts
 * matching the ASP.NET Core backend DTOs.
 * Source of truth:
 * - EngineeringKnowledgeHub.Application.DTOs.ProductDto
 * - EngineeringKnowledgeHub.Application.DTOs.TechnologyDto
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeTypeDto
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeCategoryDto
 * - EngineeringKnowledgeHub.Application.DTOs.KnowledgeTagDto
 */

export interface ProductDto {
  id: string;
  name: string;
  code: string;
  description: string;
  isActive: boolean;
}

export interface TechnologyDto {
  id: string;
  name: string;
  description: string;
}

export interface KnowledgeTypeDto {
  id: string;
  name: string;
  description: string;
}

export interface KnowledgeCategoryDto {
  id: string;
  name: string;
  description: string;
  parentCategoryId: string | null;
}

export interface KnowledgeTagDto {
  id: string;
  name: string;
}

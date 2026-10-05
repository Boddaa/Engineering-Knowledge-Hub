import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type {
  ProductDto,
  TechnologyDto,
  KnowledgeTypeDto,
  KnowledgeCategoryDto,
  KnowledgeTagDto,
} from '../types/taxonomyTypes';

/**
 * Taxonomy and Metadata Feature API Service.
 * Strongly-typed API client consuming confirmed backend ASP.NET Core endpoints
 * for products, technologies, and knowledge taxonomy.
 *
 * NOTE: The backend only provides single-item retrieval endpoints for these entities
 * (GET /api/products/{id}, GET /api/technologies/{id}, GET /api/taxonomy/{types|categories|tags}/{id}).
 * Collection listing endpoints (GET /api/products, etc.) are NOT implemented on the backend.
 * In accordance with contract-first governance, we do not invent fake list endpoints or mock data.
 */
export const taxonomyApi = {
  /**
   * Retrieves a product by its unique identifier.
   * Endpoint: GET /api/products/{id}
   */
  getProduct(id: string, options?: RequestOptions): Promise<ProductDto> {
    return apiClient.get<ProductDto>(`/api/products/${id}`, options);
  },

  /**
   * Retrieves a technology by its unique identifier.
   * Endpoint: GET /api/technologies/{id}
   */
  getTechnology(id: string, options?: RequestOptions): Promise<TechnologyDto> {
    return apiClient.get<TechnologyDto>(`/api/technologies/${id}`, options);
  },

  /**
   * Retrieves a knowledge type by its unique identifier.
   * Endpoint: GET /api/taxonomy/types/{id}
   */
  getKnowledgeType(id: string, options?: RequestOptions): Promise<KnowledgeTypeDto> {
    return apiClient.get<KnowledgeTypeDto>(`/api/taxonomy/types/${id}`, options);
  },

  /**
   * Retrieves a knowledge category by its unique identifier.
   * Endpoint: GET /api/taxonomy/categories/{id}
   */
  getCategory(id: string, options?: RequestOptions): Promise<KnowledgeCategoryDto> {
    return apiClient.get<KnowledgeCategoryDto>(`/api/taxonomy/categories/${id}`, options);
  },

  /**
   * Retrieves a knowledge tag by its unique identifier.
   * Endpoint: GET /api/taxonomy/tags/{id}
   */
  getTag(id: string, options?: RequestOptions): Promise<KnowledgeTagDto> {
    return apiClient.get<KnowledgeTagDto>(`/api/taxonomy/tags/${id}`, options);
  },
};

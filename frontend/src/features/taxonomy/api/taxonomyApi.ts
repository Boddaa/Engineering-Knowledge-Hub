import { apiClient, type RequestOptions } from '@/services/api/apiClient';
import type {
  ProductDto,
  TechnologyDto,
  KnowledgeTypeDto,
  KnowledgeCategoryDto,
  KnowledgeTagDto,
  TaxonomySeedResultDto,
  CreateProductRequest,
  CreateTechnologyRequest,
  CreateCategoryRequest,
  CreateKnowledgeTypeRequest,
  CreateKnowledgeTagRequest,
} from '../types/taxonomyTypes';

/**
 * Taxonomy, Product, and Technology Feature API Service.
 * Strongly-typed API client consuming confirmed backend ASP.NET Core endpoints.
 */
export const taxonomyApi = {
  // ================= Single-Item Retrieval =================

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

  // ================= Collection Listing =================

  /**
   * Retrieves all products, optionally filtered by active status.
   * Endpoint: GET /api/products[?isActive=true|false]
   */
  getProducts(isActive?: boolean, options?: RequestOptions): Promise<ProductDto[]> {
    const query = isActive !== undefined ? `?isActive=${isActive}` : '';
    return apiClient.get<ProductDto[]>(`/api/products${query}`, options);
  },

  /**
   * Retrieves all technologies.
   * Endpoint: GET /api/technologies
   */
  getTechnologies(options?: RequestOptions): Promise<TechnologyDto[]> {
    return apiClient.get<TechnologyDto[]>('/api/technologies', options);
  },

  /**
   * Retrieves all knowledge categories.
   * Endpoint: GET /api/taxonomy/categories
   */
  getCategories(options?: RequestOptions): Promise<KnowledgeCategoryDto[]> {
    return apiClient.get<KnowledgeCategoryDto[]>('/api/taxonomy/categories', options);
  },

  /**
   * Retrieves all knowledge types.
   * Endpoint: GET /api/taxonomy/types
   */
  getKnowledgeTypes(options?: RequestOptions): Promise<KnowledgeTypeDto[]> {
    return apiClient.get<KnowledgeTypeDto[]>('/api/taxonomy/types', options);
  },

  /**
   * Retrieves all knowledge tags.
   * Endpoint: GET /api/taxonomy/tags
   */
  getTags(options?: RequestOptions): Promise<KnowledgeTagDto[]> {
    return apiClient.get<KnowledgeTagDto[]>('/api/taxonomy/tags', options);
  },

  // ================= Idempotent Seeding =================

  /**
   * Seeds default engineering taxonomy (types, categories, products, technologies, tags).
   * Endpoint: POST /api/taxonomy/seed
   */
  seedDefaults(options?: RequestOptions): Promise<TaxonomySeedResultDto> {
    return apiClient.post<TaxonomySeedResultDto>('/api/taxonomy/seed', {}, options);
  },

  // ================= Creation Endpoints =================

  /**
   * Creates a new product.
   * Endpoint: POST /api/products
   */
  createProduct(data: CreateProductRequest, options?: RequestOptions): Promise<ProductDto> {
    return apiClient.post<ProductDto>('/api/products', data, options);
  },

  /**
   * Creates a new technology.
   * Endpoint: POST /api/technologies
   */
  createTechnology(data: CreateTechnologyRequest, options?: RequestOptions): Promise<TechnologyDto> {
    return apiClient.post<TechnologyDto>('/api/technologies', data, options);
  },

  /**
   * Creates a new knowledge category.
   * Endpoint: POST /api/taxonomy/categories
   */
  createCategory(data: CreateCategoryRequest, options?: RequestOptions): Promise<KnowledgeCategoryDto> {
    return apiClient.post<KnowledgeCategoryDto>('/api/taxonomy/categories', data, options);
  },

  /**
   * Creates a new knowledge type.
   * Endpoint: POST /api/taxonomy/types
   */
  createKnowledgeType(data: CreateKnowledgeTypeRequest, options?: RequestOptions): Promise<KnowledgeTypeDto> {
    return apiClient.post<KnowledgeTypeDto>('/api/taxonomy/types', data, options);
  },

  /**
   * Creates a new knowledge tag.
   * Endpoint: POST /api/taxonomy/tags
   */
  createTag(data: CreateKnowledgeTagRequest, options?: RequestOptions): Promise<KnowledgeTagDto> {
    return apiClient.post<KnowledgeTagDto>('/api/taxonomy/tags', data, options);
  },
};

import { useState, useEffect, useCallback, useRef } from 'react';
import { taxonomyApi } from '../api/taxonomyApi';
import type {
  ProductDto,
  TechnologyDto,
  KnowledgeTypeDto,
  KnowledgeCategoryDto,
  KnowledgeTagDto,
  TaxonomySeedResultDto,
} from '../types/taxonomyTypes';

export interface TaxonomyCatalogState {
  products: ProductDto[];
  technologies: TechnologyDto[];
  types: KnowledgeTypeDto[];
  categories: KnowledgeCategoryDto[];
  tags: KnowledgeTagDto[];
  isLoading: boolean;
  isSeeding: boolean;
  error: string | null;
  seedCatalog: () => Promise<TaxonomySeedResultDto | null>;
  refreshCatalog: () => Promise<void>;
}

/**
 * Custom React hook for fetching and caching the entire taxonomy catalog
 * (KnowledgeTypes, Categories, Products, Technologies, and Tags) in parallel.
 * Provides a seedCatalog helper that invokes POST /api/taxonomy/seed and refreshes state.
 */
export function useTaxonomyCatalog(): TaxonomyCatalogState {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [technologies, setTechnologies] = useState<TechnologyDto[]>([]);
  const [types, setTypes] = useState<KnowledgeTypeDto[]>([]);
  const [categories, setCategories] = useState<KnowledgeCategoryDto[]>([]);
  const [tags, setTags] = useState<KnowledgeTagDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSeeding, setIsSeeding] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchCatalog = useCallback(async () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const [prods, techs, typs, cats, tgs] = await Promise.all([
        taxonomyApi.getProducts(undefined, { signal: controller.signal }),
        taxonomyApi.getTechnologies({ signal: controller.signal }),
        taxonomyApi.getKnowledgeTypes({ signal: controller.signal }),
        taxonomyApi.getCategories({ signal: controller.signal }),
        taxonomyApi.getTags({ signal: controller.signal }),
      ]);

      setProducts(prods);
      setTechnologies(techs);
      setTypes(typs);
      setCategories(cats);
      setTags(tgs);
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') {
        return;
      }
      const message = err instanceof Error ? err.message : 'Failed to load taxonomy catalog.';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCatalog();
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchCatalog]);

  const seedCatalog = useCallback(async (): Promise<TaxonomySeedResultDto | null> => {
    setIsSeeding(true);
    setError(null);
    try {
      const result = await taxonomyApi.seedDefaults();
      await fetchCatalog();
      return result;
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to seed taxonomy catalog.';
      setError(message);
      return null;
    } finally {
      setIsSeeding(false);
    }
  }, [fetchCatalog]);

  return {
    products,
    technologies,
    types,
    categories,
    tags,
    isLoading,
    isSeeding,
    error,
    seedCatalog,
    refreshCatalog: fetchCatalog,
  };
}

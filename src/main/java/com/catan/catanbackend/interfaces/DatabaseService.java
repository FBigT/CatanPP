package com.catan.catanbackend.interfaces;

import java.util.List;

public interface DatabaseService<T> {
    T findById(Long id);
    void update(T entity);
    void deleteById(Long id);
    List<T> getAll();
    T create(T entity);
}
